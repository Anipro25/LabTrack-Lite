using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using LabTrack.Api.Data;
using LabTrack.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Resolve JWT settings from either flat (JWT_*) or nested (Jwt:*) env vars, preferring flat to avoid bad defaults
string signingKey = config["JWT_SIGNING_KEY"] ?? config["Jwt:SigningKey"] ?? string.Empty;
string issuer = config["JWT_ISSUER"] ?? config["Jwt:Issuer"] ?? "labtrack-lite";
string audience = config["JWT_AUDIENCE"] ?? config["Jwt:Audience"] ?? "labtrack-lite-clients";
int expiresMinutes = int.TryParse(config["JWT_EXPIRES_MINUTES"] ?? config["Jwt:ExpiresMinutes"], out var mins) ? mins : 60;

// Validate JWT signing key early to avoid runtime 500s on login
if (Encoding.UTF8.GetBytes(signingKey).Length < 32)
{
    Console.Error.WriteLine("JWT signing key is too short (<256 bits). Provide a strong key via Jwt:SigningKey or JWT_SIGNING_KEY.");
}

// Use in-memory database for hackathon demo (switch to PostgreSQL if explicitly set)
var usePostgres = Environment.GetEnvironmentVariable("DB_TYPE")?.ToLower() == "postgres";

if (usePostgres)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(config.GetConnectionString("Default")));
}
else
{
    // Default to in-memory for hackathon
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("LabTrackDb"));
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(Role.Admin.ToString()));
    options.AddPolicy("Engineer", policy => policy.RequireRole(Role.Engineer.ToString(), Role.Admin.ToString()));
    options.AddPolicy("Technician", policy => policy.RequireRole(Role.Technician.ToString(), Role.Engineer.ToString(), Role.Admin.ToString()));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
        policy.WithOrigins(config["VITE_API_BASE"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Seed database on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        if (!db.Users.Any())
        {
            SeedData.SeedDatabase(db);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Seeding error (non-fatal): {ex.Message}");
}

app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/auth/login", (LoginRequest req) =>
{
    if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Role))
        return Results.BadRequest("Email and role required");

    // Parse role string to enum
    if (!Enum.TryParse<Role>(req.Role, ignoreCase: true, out var role))
        return Results.BadRequest($"Invalid role: {req.Role}. Must be Admin, Engineer, or Technician");

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, req.Email),
        new Claim(ClaimTypes.Role, role.ToString()),
        new Claim("name", req.Email)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
        signingCredentials: creds);

    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
});

app.MapGet("/api/assets", async (AppDbContext db, int page = 1, int pageSize = 20) =>
{
    var query = db.Assets.AsNoTracking().OrderBy(a => a.Name);
    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    return Results.Ok(new { total, items });
}).RequireAuthorization("Technician");

app.MapPost("/api/assets", async (AppDbContext db, CreateAssetRequest req) =>
{
    var asset = new Asset
    {
        Name = req.Name,
        Code = req.Code ?? string.Empty,
        Location = req.Location ?? string.Empty,
        Category = req.Category ?? string.Empty,
        Description = req.Description ?? string.Empty
    };
    db.Assets.Add(asset);
    await db.SaveChangesAsync();
    return Results.Created($"/api/assets/{asset.Id}", asset);
}).RequireAuthorization("Engineer");

app.MapPut("/api/assets/{id:guid}", async (Guid id, AppDbContext db, UpdateAssetRequest req) =>
{
    var asset = await db.Assets.FindAsync(id);
    if (asset is null) return Results.NotFound();
    asset.Name = req.Name ?? asset.Name;
    asset.Code = req.Code ?? asset.Code;
    asset.Location = req.Location ?? asset.Location;
    asset.Category = req.Category ?? asset.Category;
    asset.Description = req.Description ?? asset.Description;
    await db.SaveChangesAsync();
    return Results.Ok(asset);
}).RequireAuthorization("Engineer");

app.MapDelete("/api/assets/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var asset = await db.Assets.FindAsync(id);
    if (asset is null) return Results.NotFound();
    db.Assets.Remove(asset);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.MapGet("/api/tickets", async (AppDbContext db, TicketStatus? status, int page = 1, int pageSize = 20) =>
{
    IQueryable<Ticket> query = db.Tickets.AsNoTracking().Include(t => t.Asset);
    if (status.HasValue) query = query.Where(t => t.Status == status.Value);
    var orderedQuery = query.OrderByDescending(t => t.CreatedAt);
    var total = await query.CountAsync();
    var items = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize)
        .Select(t => new
        {
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.CreatedAt,
            t.UpdatedAt,
            t.AssetId,
            Asset = t.Asset == null ? null : new { t.Asset.Id, t.Asset.Name, t.Asset.Code, t.Asset.Location }
        })
        .ToListAsync();
    return Results.Ok(new { total, items });
}).RequireAuthorization("Technician");

app.MapPost("/api/tickets", async (AppDbContext db, CreateTicketRequest req, ClaimsPrincipal user) =>
{
    var createdBy = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.Identity?.Name ?? "unknown";
    var ticket = new Ticket
    {
        Title = req.Title,
        Description = req.Description ?? string.Empty,
        AssetId = req.AssetId,
        Status = TicketStatus.Open,
        CreatedByUserId = Guid.NewGuid()
    };
    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tickets/{ticket.Id}", ticket);
}).RequireAuthorization("Technician");

// Update core fields of a ticket (title/description/asset/status)
app.MapPut("/api/tickets/{id:guid}", async (Guid id, AppDbContext db, UpdateTicketRequest req) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();

    ticket.Title = req.Title ?? ticket.Title;
    ticket.Description = req.Description ?? ticket.Description;
    if (req.AssetId.HasValue) ticket.AssetId = req.AssetId.Value;
    if (req.Status.HasValue) ticket.Status = req.Status.Value;
    ticket.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(ticket);
}).RequireAuthorization("Engineer");

// Delete a ticket
app.MapDelete("/api/tickets/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();
    db.Tickets.Remove(ticket);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.MapPut("/api/tickets/{id:guid}/status", async (Guid id, AppDbContext db, UpdateTicketStatusRequest req) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();
    ticket.Status = req.Status;
    ticket.UpdatedAt = DateTime.UtcNow;
    if (req.AssignedToUserId.HasValue)
    {
        ticket.AssignedToUserId = req.AssignedToUserId;
    }
    await db.SaveChangesAsync();
    return Results.Ok(ticket);
}).RequireAuthorization("Engineer");

app.MapPost("/api/tickets/{id:guid}/comments", async (Guid id, AppDbContext db, CreateCommentRequest req, ClaimsPrincipal user) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) return Results.NotFound();
    var comment = new Comment
    {
        TicketId = id,
        AuthorId = Guid.NewGuid(),
        Body = req.Body
    };
    db.Comments.Add(comment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tickets/{id}/comments/{comment.Id}", comment);
}).RequireAuthorization("Technician");

app.MapGet("/api/chatbot", async (string q, AppDbContext db) =>
{
    var text = q.ToLowerInvariant();
    
    // Asset queries
    if (text.Contains("how many asset") || text.Contains("count asset") || text.Contains("total asset"))
    {
        var count = await db.Assets.CountAsync();
        return Results.Ok($"There are {count} assets in the system.");
    }
    if (text.Contains("list asset") || text.Contains("show asset") || text.Contains("all asset"))
    {
        var assets = await db.Assets.Take(5).Select(a => a.Name).ToListAsync();
        return Results.Ok($"Recent assets: {string.Join(", ", assets)}. Visit the Assets page for more.");
    }
    if (text.Contains("lab a") || text.Contains("location"))
    {
        var labA = await db.Assets.Where(a => a.Location.Contains("Lab A")).Select(a => a.Name).ToListAsync();
        return Results.Ok(labA.Any() ? $"Assets in Lab A: {string.Join(", ", labA)}" : "No assets found in Lab A.");
    }
    
    // Ticket queries
    if (text.Contains("open ticket") || text.Contains("pending ticket"))
    {
        var open = await db.Tickets.CountAsync(t => t.Status == TicketStatus.Open);
        return Results.Ok($"There are {open} open tickets. Visit the Tickets page to view them.");
    }
    if (text.Contains("ticket") && (text.Contains("how many") || text.Contains("count") || text.Contains("total")))
    {
        var count = await db.Tickets.CountAsync();
        return Results.Ok($"There are {count} total tickets in the system.");
    }
    if (text.Contains("create ticket") || text.Contains("new ticket") || text.Contains("add ticket"))
    {
        return Results.Ok("To create a ticket, go to the Tickets page and use the 'Create New Ticket' form.");
    }
    if (text.Contains("ticket") && text.Contains("status"))
    {
        var grouped = await db.Tickets.GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();
        var summary = string.Join(", ", grouped.Select(g => $"{g.Count} {g.Status}"));
        return Results.Ok($"Ticket status breakdown: {summary}");
    }
    
    // General queries
    if (text.Contains("help") || text.Contains("what can") || text.Contains("how to"))
    {
        return Results.Ok("I can help you with: asset counts, ticket status, open tickets, location-based asset queries. Try asking 'How many assets?' or 'Show open tickets'.");
    }
    
    return Results.Ok("I'm not sure about that. Try asking about assets, tickets, or type 'help' for suggestions.");
}).RequireAuthorization("Technician");

app.Run();

record LoginRequest(string Email, string Role);
record CreateAssetRequest(string Name, string? Code, string? Location, string? Category, string? Description);
record UpdateAssetRequest(string? Name, string? Code, string? Location, string? Category, string? Description);
record CreateTicketRequest(string Title, string? Description, Guid AssetId);
record UpdateTicketRequest(string? Title, string? Description, Guid? AssetId, TicketStatus? Status);
record UpdateTicketStatusRequest(TicketStatus Status, Guid? AssignedToUserId);
record CreateCommentRequest(string Body);
