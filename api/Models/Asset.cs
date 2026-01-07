using System.ComponentModel.DataAnnotations;

namespace LabTrack.Api.Models;

public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Code { get; set; } = string.Empty; // QR/asset code

    [MaxLength(120)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
