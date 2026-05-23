using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Application.Features.Scoring;
using CourtBooking.Application.Features.Scoring.DTOs;
using CourtBooking.Domain.Entities.Authentication;
using CourtBooking.Domain.Entities.Scoring;
using Microsoft.AspNetCore.Identity;

namespace CourtBooking.Infrastructure.Services.Scoring;

public class PickleballScoringEngine : IScoringEngineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public PickleballScoringEngine(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<ScoringMatchDto> StartMatchAsync(Guid matchId, string userId)
    {
        var match = await GetMatchOrThrow(matchId);
        await EnsureCanControlMatch(match, userId);

        if (match.Status != "Created" && match.Status != "Ready")
            throw new InvalidOperationException("Match can only be started from Created or Ready status.");

        var isDoubles = match.GameType == "Doubles";
        match.ServingTeam = "A";
        match.ServerNumber = isDoubles ? 2 : null;
        match.ScoreCall = isDoubles ? "0-0-2" : "0-0";
        match.TeamAScore = 0;
        match.TeamBScore = 0;
        match.Status = "InProgress";
        match.StartedAt = DateTime.UtcNow;
        match.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ScoringMatch>().Update(match);

        var evt = new ScoringEvent
        {
            MatchId = match.Id,
            SequenceNumber = await NextSequence(match.Id),
            EventType = "MatchStarted",
            NewTeamAScore = 0,
            NewTeamBScore = 0,
            NewServingTeam = match.ServingTeam,
            NewServerNumber = match.ServerNumber,
            NewScoreCall = match.ScoreCall,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Repository<ScoringEvent>().Add(evt);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(match.Id);
    }

    public async Task<ScoringMatchDto> ApplyRallyAsync(Guid matchId, string winningTeamCode, string userId)
    {
        if (winningTeamCode != "A" && winningTeamCode != "B")
            throw new ArgumentException("Winning team must be 'A' or 'B'.");

        var match = await GetMatchOrThrow(matchId);
        await EnsureCanControlMatch(match, userId);

        if (match.Status != "InProgress")
            throw new InvalidOperationException("Match is not in progress.");

        var isDoubles = match.GameType == "Doubles";
        var prevTeamAScore = match.TeamAScore;
        var prevTeamBScore = match.TeamBScore;
        var prevServingTeam = match.ServingTeam;
        var prevServerNumber = match.ServerNumber;
        var prevScoreCall = match.ScoreCall;

        if (winningTeamCode == match.ServingTeam)
        {
            if (winningTeamCode == "A") match.TeamAScore++;
            else match.TeamBScore++;
        }
        else
        {
            if (isDoubles)
            {
                if (match.ServerNumber == 1)
                    match.ServerNumber = 2;
                else
                {
                    match.ServingTeam = match.ServingTeam == "A" ? "B" : "A";
                    match.ServerNumber = 1;
                }
            }
            else
            {
                match.ServingTeam = match.ServingTeam == "A" ? "B" : "A";
                match.ServerNumber = null;
            }
        }

        match.ScoreCall = BuildScoreCall(match);

        if (CheckWinCondition(match))
        {
            match.Status = "Completed";
            match.WinnerTeam = winningTeamCode;
            match.CompletedAt = DateTime.UtcNow;
        }

        match.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ScoringMatch>().Update(match);

        var evt = new ScoringEvent
        {
            MatchId = match.Id,
            SequenceNumber = await NextSequence(match.Id),
            EventType = match.Status == "Completed" ? "MatchEnded" : "Rally",
            RallyWinnerTeam = winningTeamCode,
            PreviousTeamAScore = prevTeamAScore,
            PreviousTeamBScore = prevTeamBScore,
            NewTeamAScore = match.TeamAScore,
            NewTeamBScore = match.TeamBScore,
            PreviousServingTeam = prevServingTeam,
            NewServingTeam = match.ServingTeam,
            PreviousServerNumber = prevServerNumber,
            NewServerNumber = match.ServerNumber,
            PreviousScoreCall = prevScoreCall,
            NewScoreCall = match.ScoreCall,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Repository<ScoringEvent>().Add(evt);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(match.Id);
    }

    public async Task<ScoringMatchDto> UndoLastRallyAsync(Guid matchId, string userId)
    {
        var match = await GetMatchOrThrow(matchId);
        await EnsureCanControlMatch(match, userId);

        var events = await _unitOfWork.Repository<ScoringEvent>()
            .FindAsync(e => e.MatchId == matchId && !e.IsUndone && e.EventType != "MatchCreated" && e.EventType != "MatchStarted");

        var lastEvent = events.OrderByDescending(e => e.SequenceNumber).FirstOrDefault();

        if (lastEvent == null)
            throw new InvalidOperationException("No rally events to undo.");

        match.TeamAScore = lastEvent.PreviousTeamAScore;
        match.TeamBScore = lastEvent.PreviousTeamBScore;
        match.ServingTeam = lastEvent.PreviousServingTeam;
        match.ServerNumber = lastEvent.PreviousServerNumber;
        match.ScoreCall = lastEvent.PreviousScoreCall;

        if (match.Status == "Completed")
        {
            match.Status = "InProgress";
            match.WinnerTeam = null;
            match.CompletedAt = null;
        }

        match.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ScoringMatch>().Update(match);

        lastEvent.IsUndone = true;
        lastEvent.UndoneAt = DateTime.UtcNow;
        _unitOfWork.Repository<ScoringEvent>().Update(lastEvent);

        var undoEvent = new ScoringEvent
        {
            MatchId = match.Id,
            SequenceNumber = await NextSequence(match.Id),
            EventType = "Undo",
            PreviousTeamAScore = lastEvent.NewTeamAScore,
            PreviousTeamBScore = lastEvent.NewTeamBScore,
            NewTeamAScore = lastEvent.PreviousTeamAScore,
            NewTeamBScore = lastEvent.PreviousTeamBScore,
            PreviousServingTeam = lastEvent.NewServingTeam,
            NewServingTeam = lastEvent.PreviousServingTeam,
            PreviousServerNumber = lastEvent.NewServerNumber,
            NewServerNumber = lastEvent.PreviousServerNumber,
            PreviousScoreCall = lastEvent.NewScoreCall,
            NewScoreCall = lastEvent.PreviousScoreCall,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Repository<ScoringEvent>().Add(undoEvent);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(match.Id);
    }

    public async Task<ScoringMatchDto> EndMatchAsync(Guid matchId, string reason, string userId)
    {
        var match = await GetMatchOrThrow(matchId);
        await EnsureCanControlMatch(match, userId);

        if (match.Status == "Completed")
            throw new InvalidOperationException("Match is already completed.");

        if (match.TeamAScore > match.TeamBScore)
            match.WinnerTeam = "A";
        else if (match.TeamBScore > match.TeamAScore)
            match.WinnerTeam = "B";

        match.Status = "Completed";
        match.CompletedAt = DateTime.UtcNow;
        match.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ScoringMatch>().Update(match);

        var evt = new ScoringEvent
        {
            MatchId = match.Id,
            SequenceNumber = await NextSequence(match.Id),
            EventType = "MatchEnded",
            PreviousTeamAScore = match.TeamAScore,
            PreviousTeamBScore = match.TeamBScore,
            NewTeamAScore = match.TeamAScore,
            NewTeamBScore = match.TeamBScore,
            PreviousServingTeam = match.ServingTeam,
            NewServingTeam = match.ServingTeam,
            PreviousServerNumber = match.ServerNumber,
            NewServerNumber = match.ServerNumber,
            PreviousScoreCall = match.ScoreCall,
            NewScoreCall = match.ScoreCall,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
        };

        _unitOfWork.Repository<ScoringEvent>().Add(evt);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(match.Id);
    }

    private async Task EnsureCanControlMatch(ScoringMatch match, string userId)
    {
        if (match.CreatedByUserId == userId)
            return;

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return;
        }

        throw new UnauthorizedAccessException("You do not have permission to control this match.");
    }

    private async Task<ScoringMatch> GetMatchOrThrow(Guid matchId)
    {
        var match = await _unitOfWork.Repository<ScoringMatch>()
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null)
            throw new KeyNotFoundException($"Match '{matchId}' not found.");

        return match;
    }

    private async Task<int> NextSequence(Guid matchId)
    {
        var events = await _unitOfWork.Repository<ScoringEvent>()
            .FindAsync(e => e.MatchId == matchId);

        return events.Any() ? events.Max(e => e.SequenceNumber) + 1 : 1;
    }

    private static string BuildScoreCall(ScoringMatch match)
    {
        var servingScore = match.ServingTeam == "A" ? match.TeamAScore : match.TeamBScore;
        var receivingScore = match.ServingTeam == "A" ? match.TeamBScore : match.TeamAScore;

        if (match.GameType == "Doubles")
            return $"{servingScore}-{receivingScore}-{match.ServerNumber}";

        return $"{servingScore}-{receivingScore}";
    }

    private static bool CheckWinCondition(ScoringMatch match)
    {
        var servingScore = match.ServingTeam == "A" ? match.TeamAScore : match.TeamBScore;
        var receivingScore = match.ServingTeam == "A" ? match.TeamBScore : match.TeamAScore;

        return servingScore >= match.TargetScore && (servingScore - receivingScore) >= match.WinBy;
    }

    private async Task<ScoringMatchDto> MapToDto(Guid matchId)
    {
        var match = await _unitOfWork.Repository<ScoringMatch>()
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException($"Match '{matchId}' not found.");

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
            SportCode = "",
            SportName = "",
            RuleSetCode = "",
            RuleSetName = "",
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
