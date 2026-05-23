using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBooking.Domain.Entities.Scoring;

public class ScoringTeam
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MatchId { get; set; }

    [Required, MaxLength(10)]
    public string TeamCode { get; set; } = string.Empty; // A or B

    [Required, MaxLength(100)]
    public string TeamName { get; set; } = string.Empty;

    public int Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(MatchId))]
    public ScoringMatch? Match { get; set; }

    public ICollection<ScoringPlayer> Players { get; set; } = new List<ScoringPlayer>();
}
