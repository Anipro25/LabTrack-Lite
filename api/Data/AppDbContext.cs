using LabTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.Code);
        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.Location);

        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Status);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.AssetId);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.AssignedToUserId);

        modelBuilder.Entity<Comment>()
            .HasIndex(c => c.TicketId);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Asset)
            .WithMany(a => a.Tickets)
            .HasForeignKey(t => t.AssetId);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
