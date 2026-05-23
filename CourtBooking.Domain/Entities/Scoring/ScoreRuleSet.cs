using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBooking.Domain.Entities.Scoring;

public class ScoreRuleSet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SportId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ScoringMode { get; set; } = string.Empty; // SideOut, Rally

    public int TargetScore { get; set; }
    public int WinBy { get; set; } = 2;
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(SportId))]
    public ScoreSport? Sport { get; set; }
}
