using System.Security.Claims;
using CourtBooking.Application.Features.Scoring;
using CourtBooking.Application.Features.Scoring.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourtBooking.API.Controllers;

[ApiController]
[Route("api/scoring")]
[Authorize]
public class ScoringMatchesController : ControllerBase
{
    private readonly IScoringMatchService _scoringMatchService;

    public ScoringMatchesController(IScoringMatchService scoringMatchService)
    {
        _scoringMatchService = scoringMatchService;
    }

    [HttpPost("matches")]
    public async Task<IActionResult> CreateMatch([FromBody] CreateScoringMatchRequestDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var match = await _scoringMatchService.CreateMatchAsync(request, userId);
            return Ok(match);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("matches/{id:guid}")]
    public async Task<IActionResult> GetMatchById(Guid id)
    {
        var match = await _scoringMatchService.GetMatchByIdAsync(id);
        if (match == null)
            return NotFound(new { message = "Match not found." });

        return Ok(match);
    }

    [HttpGet("matches/my-history")]
    public async Task<IActionResult> GetMyHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var matches = await _scoringMatchService.GetMyMatchHistoryAsync(userId);
        return Ok(matches);
    }

    [HttpGet("players/search")]
    public async Task<IActionResult> SearchPlayers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(new List<PlayerSearchResultDto>());

        var results = await _scoringMatchService.SearchPlayersAsync(query);
        return Ok(results);
    }
}
