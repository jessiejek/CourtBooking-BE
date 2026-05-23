using CourtBooking.Application.Features.Scoring.DTOs;

namespace CourtBooking.Application.Features.Scoring;

public interface IScoringMatchService
{
    Task<ScoringMatchDto> CreateMatchAsync(CreateScoringMatchRequestDto request, string userId);
    Task<ScoringMatchDto?> GetMatchByIdAsync(Guid matchId);
    Task<List<ScoringMatchDto>> GetMyMatchHistoryAsync(string userId);
    Task<List<PlayerSearchResultDto>> SearchPlayersAsync(string query);
}
