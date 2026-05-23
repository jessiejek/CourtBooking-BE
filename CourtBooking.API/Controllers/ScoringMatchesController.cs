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
    private readonly IScoringEngineService _scoringEngineService;

    public ScoringMatchesController(
        IScoringMatchService scoringMatchService,
        IScoringEngineService scoringEngineService)
    {
        _scoringMatchService = scoringMatchService;
        _scoringEngineService = scoringEngineService;
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

    [HttpPost("matches/{id:guid}/start")]
    public async Task<IActionResult> StartMatch(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var match = await _scoringEngineService.StartMatchAsync(id, userId);
            return Ok(match);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("matches/{id:guid}/rally")]
    public async Task<IActionResult> ApplyRally(Guid id, [FromBody] RallyRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var match = await _scoringEngineService.ApplyRallyAsync(id, request.WinningTeamCode, userId);
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

    [HttpPost("matches/{id:guid}/undo")]
    public async Task<IActionResult> UndoRally(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var match = await _scoringEngineService.UndoLastRallyAsync(id, userId);
            return Ok(match);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("matches/{id:guid}/end")]
    public async Task<IActionResult> EndMatch(Guid id, [FromBody] EndMatchRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            var match = await _scoringEngineService.EndMatchAsync(id, request.Reason, userId);
            return Ok(match);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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

public class RallyRequest
{
    public string WinningTeamCode { get; set; } = string.Empty;
}

public class EndMatchRequest
{
    public string Reason { get; set; } = string.Empty;
}
