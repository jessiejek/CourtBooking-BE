using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBooking.Domain.Entities.Scoring;

public class ScoringPlayer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MatchId { get; set; }

    [Required]
    public Guid TeamId { get; set; }

    public string? RegisteredUserId { get; set; }

    [Required, MaxLength(100)]
    public string PlayerName { get; set; } = string.Empty;

    public int PlayerOrder { get; set; }

    public bool IsGuest { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(MatchId))]
    public ScoringMatch? Match { get; set; }

    [ForeignKey(nameof(TeamId))]
    public ScoringTeam? Team { get; set; }
}
