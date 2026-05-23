using CourtBooking.Application.Features.Scoring.DTOs;

namespace CourtBooking.Application.Features.Scoring;

public interface IScoringValidationService
{
    void ValidateCreateMatchRequest(CreateScoringMatchRequestDto request);
}
