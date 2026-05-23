using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBooking.Domain.Entities.Scoring;

public class ScoringMatch
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? BookingId { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    [Required]
    public Guid SportId { get; set; }

    [Required]
    public Guid RuleSetId { get; set; }

    [Required, MaxLength(20)]
    public string MatchMode { get; set; } = string.Empty; // OpenPlay, Booking

    [Required, MaxLength(20)]
    public string GameType { get; set; } = string.Empty;   // Singles, Doubles

    public int TargetScore { get; set; } = 11;
    public int WinBy { get; set; } = 2;

    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }

    [MaxLength(10)]
    public string ServingTeam { get; set; } = "A";

    public int? ServerNumber { get; set; }

    public Guid? CurrentServerPlayerId { get; set; }

    [MaxLength(20)]
    public string ScoreCall { get; set; } = "0-0";

    [Required, MaxLength(20)]
    public string Status { get; set; } = "InProgress";

    [MaxLength(10)]
    public string? WinnerTeam { get; set; }

    public bool IsOpenPlay { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(SportId))]
    public ScoreSport? Sport { get; set; }

    [ForeignKey(nameof(RuleSetId))]
    public ScoreRuleSet? RuleSet { get; set; }

    public ICollection<ScoringTeam> Teams { get; set; } = new List<ScoringTeam>();
    public ICollection<ScoringEvent> Events { get; set; } = new List<ScoringEvent>();
}
