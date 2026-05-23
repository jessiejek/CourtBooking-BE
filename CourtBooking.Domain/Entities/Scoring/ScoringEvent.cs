using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourtBooking.Domain.Entities.Scoring;

public class ScoringEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MatchId { get; set; }

    public int SequenceNumber { get; set; }

    [MaxLength(10)]
    public string? RallyWinnerTeam { get; set; }

    public int PreviousTeamAScore { get; set; }
    public int PreviousTeamBScore { get; set; }
    public int NewTeamAScore { get; set; }
    public int NewTeamBScore { get; set; }

    [MaxLength(10)]
    public string PreviousServingTeam { get; set; } = string.Empty;

    [MaxLength(10)]
    public string NewServingTeam { get; set; } = string.Empty;

    public int? PreviousServerNumber { get; set; }
    public int? NewServerNumber { get; set; }

    [MaxLength(20)]
    public string PreviousScoreCall { get; set; } = string.Empty;

    [MaxLength(20)]
    public string NewScoreCall { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string EventType { get; set; } = string.Empty; // MatchCreated, Rally, Undo, Reset, EndMatch

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(MatchId))]
    public ScoringMatch? Match { get; set; }
}
