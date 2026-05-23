using CourtBooking.Application.Features.Scoring.DTOs;

namespace CourtBooking.Application.Features.Scoring;

public interface IScoringEngineService
{
    Task<ScoringMatchDto> StartMatchAsync(Guid matchId, string userId);
    Task<ScoringMatchDto> ApplyRallyAsync(Guid matchId, string winningTeamCode, string userId);
    Task<ScoringMatchDto> UndoLastRallyAsync(Guid matchId, string userId);
    Task<ScoringMatchDto> EndMatchAsync(Guid matchId, string reason, string userId);
}
