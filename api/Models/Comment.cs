using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models;

public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    [Required]
    public Guid AuthorId { get; set; }
    public User? Author { get; set; }

    [Required, MaxLength(1024)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
