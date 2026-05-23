using CourtBooking.Application.Features.Scoring;
using CourtBooking.Application.Features.Scoring.DTOs;
using CourtBooking.Domain.Entities;
using CourtBooking.Domain.Entities.Scoring;
using CourtBooking.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CourtBooking.Infrastructure.Services.Scoring;

public class ScoringMatchService : IScoringMatchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScoringValidationService _validationService;
    private readonly UserManager<Domain.Entities.Authentication.ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public ScoringMatchService(
        IUnitOfWork unitOfWork,
        IScoringValidationService validationService,
        UserManager<Domain.Entities.Authentication.ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<ScoringMatchDto> CreateMatchAsync(CreateScoringMatchRequestDto request, string userId)
    {
        // Check ScoringRequiresBooking setting
        var scoringSetting = await _unitOfWork.Repository<AppSetting>()
            .FirstOrDefaultAsync(s => s.Key == "ScoringRequiresBooking");

        var requiresBooking = scoringSetting?.Value?.ToLower() == "true";

        if (requiresBooking && request.BookingId == null)
        {
            throw new InvalidOperationException("Scoring requires a valid booking.");
        }

        // Validate request
        _validationService.ValidateCreateMatchRequest(request);

        // Resolve sport and rule set
        var sport = await _unitOfWork.Repository<ScoreSport>()
            .FirstOrDefaultAsync(s => s.Code == request.SportCode && s.IsActive)
            ?? throw new KeyNotFoundException($"Sport '{request.SportCode}' not found.");

        var ruleSet = await _unitOfWork.Repository<ScoreRuleSet>()
            .FirstOrDefaultAsync(r => r.Code == request.RuleSetCode && r.IsActive)
            ?? throw new KeyNotFoundException($"Rule set '{request.RuleSetCode}' not found.");

        // Determine initial scoring values based on game type
        var isDoubles = request.GameType == "Doubles";
        var initialServerNumber = isDoubles ? 2 : (int?)null;
        var initialScoreCall = isDoubles ? "0-0-2" : "0-0";

        // Create match
        var match = new ScoringMatch
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            CreatedByUserId = userId,
            SportId = sport.Id,
            RuleSetId = ruleSet.Id,
            MatchMode = request.MatchMode,
            GameType = request.GameType,
            TargetScore = request.TargetScore,
            WinBy = request.WinBy,
            TeamAScore = 0,
            TeamBScore = 0,
            ServingTeam = "A",
            ServerNumber = initialServerNumber,
            ScoreCall = initialScoreCall,
            Status = "InProgress",
            IsOpenPlay = request.MatchMode == "OpenPlay",
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Repository<ScoringMatch>().Add(match);

        // Create teams and players
        foreach (var teamDto in request.Teams)
        {
            var team = new ScoringTeam
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                TeamCode = teamDto.TeamCode,
                TeamName = teamDto.TeamName,
                Score = 0,
                CreatedAt = DateTime.UtcNow,
            };

            _unitOfWork.Repository<ScoringTeam>().Add(team);

            int order = 1;
            foreach (var playerDto in teamDto.Players)
            {
                // Validate registered user if provided
                if (!string.IsNullOrWhiteSpace(playerDto.RegisteredUserId))
                {
                    var registeredUser = await _userManager.FindByIdAsync(playerDto.RegisteredUserId);
                    if (registeredUser == null)
                    {
                        throw new KeyNotFoundException($"Registered user '{playerDto.RegisteredUserId}' not found.");
                    }
                }

                var player = new ScoringPlayer
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    TeamId = team.Id,
                    RegisteredUserId = playerDto.RegisteredUserId,
                    PlayerName = playerDto.PlayerName,
                    PlayerOrder = order++,
                    IsGuest = playerDto.IsGuest,
                    CreatedAt = DateTime.UtcNow,
                };

                _unitOfWork.Repository<ScoringPlayer>().Add(player);
            }
        }

        // Create initial event
        var initialEvent = new ScoringEvent
        {
            Id = Guid.NewGuid(),
            MatchId = match.Id,
            SequenceNumber = 1,
            PreviousTeamAScore = 0,
            PreviousTeamBScore = 0,
            NewTeamAScore = 0,
            NewTeamBScore = 0,
            PreviousServingTeam = "A",
            NewServingTeam = "A",
            PreviousServerNumber = null,
            NewServerNumber = initialServerNumber,
            PreviousScoreCall = "",
            NewScoreCall = initialScoreCall,
            EventType = "MatchCreated",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId,
        };

        _unitOfWork.Repository<ScoringEvent>().Add(initialEvent);

        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(match.Id);
    }

    public async Task<ScoringMatchDto?> GetMatchByIdAsync(Guid matchId)
    {
        var match = await _unitOfWork.Repository<ScoringMatch>()
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null) return null;

        return await MapToDtoAsync(matchId);
    }

    public async Task<List<ScoringMatchDto>> GetMyMatchHistoryAsync(string userId)
    {
        var matches = await _unitOfWork.Repository<ScoringMatch>()
            .FindAsync(m => m.CreatedByUserId == userId);

        // Also find matches where user is tagged as a registered player
        var playerMatches = await _unitOfWork.Repository<ScoringPlayer>()
            .FindAsync(p => p.RegisteredUserId == userId);

        var playerMatchIds = playerMatches.Select(p => p.MatchId).Except(matches.Select(m => m.Id)).ToList();

        var additionalMatches = await _unitOfWork.Repository<ScoringMatch>()
            .FindAsync(m => playerMatchIds.Contains(m.Id));

        var allMatches = matches.Concat(additionalMatches)
            .OrderByDescending(m => m.CreatedAt)
            .ToList();

        var result = new List<ScoringMatchDto>();
        foreach (var match in allMatches)
        {
            result.Add(await MapToDtoAsync(match.Id));
        }

        return result;
    }

    public async Task<List<PlayerSearchResultDto>> SearchPlayersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<PlayerSearchResultDto>();

        var users = _userManager.Users
            .Where(u => u.FullName.Contains(query) || (u.Email != null && u.Email.Contains(query)))
            .Take(10)
            .ToList()
            .Select(u => new PlayerSearchResultDto
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                AvatarUrl = u.AvatarUrl,
            })
            .ToList();

        return users;
    }

    private async Task<ScoringMatchDto> MapToDtoAsync(Guid matchId)
    {
        var match = await _unitOfWork.Repository<ScoringMatch>()
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null)
            throw new KeyNotFoundException($"Match '{matchId}' not found.");

        var sport = await _unitOfWork.Repository<ScoreSport>()
            .FirstOrDefaultAsync(s => s.Id == match.SportId);

        var ruleSet = await _unitOfWork.Repository<ScoreRuleSet>()
            .FirstOrDefaultAsync(r => r.Id == match.RuleSetId);

        var teams = await _unitOfWork.Repository<ScoringTeam>()
            .FindAsync(t => t.MatchId == match.Id);

        var teamDtos = new List<ScoringTeamDto>();

        foreach (var team in teams)
        {
            var players = await _unitOfWork.Repository<ScoringPlayer>()
                .FindAsync(p => p.TeamId == team.Id);

            teamDtos.Add(new ScoringTeamDto
            {
                Id = team.Id,
                TeamCode = team.TeamCode,
                TeamName = team.TeamName,
                Score = team.Score,
                Players = players.OrderBy(p => p.PlayerOrder).Select(p => new ScoringPlayerDto
                {
                    Id = p.Id,
                    RegisteredUserId = p.RegisteredUserId,
                    PlayerName = p.PlayerName,
                    PlayerOrder = p.PlayerOrder,
                    IsGuest = p.IsGuest,
                }).ToList(),
            });
        }

        return new ScoringMatchDto
        {
            Id = match.Id,
            BookingId = match.BookingId,
            SportCode = sport?.Code ?? "",
            SportName = sport?.Name ?? "",
            RuleSetCode = ruleSet?.Code ?? "",
            RuleSetName = ruleSet?.Name ?? "",
            MatchMode = match.MatchMode,
            GameType = match.GameType,
            TargetScore = match.TargetScore,
            WinBy = match.WinBy,
            TeamAScore = match.TeamAScore,
            TeamBScore = match.TeamBScore,
            ServingTeam = match.ServingTeam,
            ServerNumber = match.ServerNumber,
            ScoreCall = match.ScoreCall,
            Status = match.Status,
            WinnerTeam = match.WinnerTeam,
            IsOpenPlay = match.IsOpenPlay,
            StartedAt = match.StartedAt,
            CompletedAt = match.CompletedAt,
            Teams = teamDtos,
        };
    }
}
