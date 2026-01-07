using LabTrack.Api.Models;

namespace LabTrack.Api.Data;

public static class SeedData
{
    public static void SeedDatabase(AppDbContext context)
    {
        if (context.Users.Any()) return; // Already seeded

        // Create demo users
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin User",
            Role = Role.Admin,
            PasswordHash = "hashed_password_demo"
        };

        var engineer = new User
        {
            Id = Guid.NewGuid(),
            Email = "engineer@example.com",
            Name = "Engineer User",
            Role = Role.Engineer,
            PasswordHash = "hashed_password_demo"
        };

        var tech = new User
        {
            Id = Guid.NewGuid(),
            Email = "tech@example.com",
            Name = "Technician User",
            Role = Role.Technician,
            PasswordHash = "hashed_password_demo"
        };

        context.Users.AddRange(admin, engineer, tech);
        context.SaveChanges();

        // Create demo assets
        var asset1 = new Asset
        {
            Id = Guid.NewGuid(),
            Name = "Oscilloscope XYZ-100",
            Code = "QR-OSC-001",
            Location = "Lab A",
            Category = "Electronics",
            Description = "Digital oscilloscope for circuit analysis"
        };

        var asset2 = new Asset
        {
            Id = Guid.NewGuid(),
            Name = "Microscope Carl Zeiss",
            Code = "QR-MIC-002",
            Location = "Lab B",
            Category = "Optics",
            Description = "High-resolution research microscope"
        };

        context.Assets.AddRange(asset1, asset2);
        context.SaveChanges();

        // Create demo tickets
        var ticket1 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Oscilloscope display flickering",
            Description = "The display intermittently flickers during measurement",
            Status = TicketStatus.Open,
            AssetId = asset1.Id,
            CreatedByUserId = tech.Id,
            AssignedToUserId = engineer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var ticket2 = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = "Microscope lens calibration needed",
            Description = "Focus mechanism needs recalibration after maintenance",
            Status = TicketStatus.InProgress,
            AssetId = asset2.Id,
            CreatedByUserId = tech.Id,
            AssignedToUserId = engineer.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Tickets.AddRange(ticket1, ticket2);
        context.SaveChanges();

        // Create demo comments
        var comment1 = new Comment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket1.Id,
            AuthorId = engineer.Id,
            Body = "Investigated the issue. Might be a power supply problem. Will test further.",
            CreatedAt = DateTime.UtcNow.AddHours(-12)
        };

        var comment2 = new Comment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket2.Id,
            AuthorId = engineer.Id,
            Body = "Started calibration procedure. Estimated completion: 2 hours.",
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        };

        context.Comments.AddRange(comment1, comment2);
        context.SaveChanges();
    }
}
