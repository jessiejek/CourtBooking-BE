using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Domain.Entities.Scoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Controllers;

[ApiController]
[Route("api/scoring")]
public class ScoringController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ScoringController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("sports")]
    public async Task<IActionResult> GetSports()
    {
        var sports = await _unitOfWork.Repository<ScoreSport>()
            .FindAsync(s => s.IsActive);
        return Ok(sports);
    }

    [HttpGet("rule-sets")]
    public async Task<IActionResult> GetRuleSets([FromQuery] string? sportCode = null)
    {
        if (!string.IsNullOrWhiteSpace(sportCode))
        {
            var sport = await _unitOfWork.Repository<ScoreSport>()
                .FirstOrDefaultAsync(s => s.Code == sportCode && s.IsActive);

            if (sport == null)
                return NotFound(new { message = "Sport not found." });

            var ruleSets = await _unitOfWork.Repository<ScoreRuleSet>()
                .FindAsync(r => r.SportId == sport.Id && r.IsActive);

            return Ok(ruleSets);
        }

        var allRuleSets = await _unitOfWork.Repository<ScoreRuleSet>()
            .FindAsync(r => r.IsActive);

        return Ok(allRuleSets);
    }
}
