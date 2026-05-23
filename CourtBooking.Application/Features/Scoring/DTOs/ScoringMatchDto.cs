namespace CourtBooking.Application.Features.Scoring.DTOs;

public class ScoringMatchDto
{
    public Guid Id { get; set; }
    public Guid? BookingId { get; set; }
    public string SportCode { get; set; } = string.Empty;
    public string SportName { get; set; } = string.Empty;
    public string RuleSetCode { get; set; } = string.Empty;
    public string RuleSetName { get; set; } = string.Empty;
    public string MatchMode { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int TargetScore { get; set; }
    public int WinBy { get; set; }
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }
    public string ServingTeam { get; set; } = string.Empty;
    public int? ServerNumber { get; set; }
    public string ScoreCall { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? WinnerTeam { get; set; }
    public bool IsOpenPlay { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ScoringTeamDto> Teams { get; set; } = new();
}

public class ScoringTeamDto
{
    public Guid Id { get; set; }
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<ScoringPlayerDto> Players { get; set; } = new();
}

public class ScoringPlayerDto
{
    public Guid Id { get; set; }
    public string? RegisteredUserId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int PlayerOrder { get; set; }
    public bool IsGuest { get; set; }
}
