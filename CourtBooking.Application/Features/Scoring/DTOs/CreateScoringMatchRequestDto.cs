namespace CourtBooking.Application.Features.Scoring.DTOs;

public class CreateScoringMatchRequestDto
{
    public Guid? BookingId { get; set; }
    public string SportCode { get; set; } = string.Empty;
    public string RuleSetCode { get; set; } = string.Empty;
    public string MatchMode { get; set; } = "OpenPlay";
    public string GameType { get; set; } = "Doubles";
    public int TargetScore { get; set; } = 11;
    public int WinBy { get; set; } = 2;
    public List<CreateTeamDto> Teams { get; set; } = new();
}

public class CreateTeamDto
{
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public List<CreatePlayerDto> Players { get; set; } = new();
}

public class CreatePlayerDto
{
    public string? RegisteredUserId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsGuest { get; set; }
}
