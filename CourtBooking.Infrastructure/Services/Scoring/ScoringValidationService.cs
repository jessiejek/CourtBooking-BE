using CourtBooking.Application.Features.Scoring;
using CourtBooking.Application.Features.Scoring.DTOs;

namespace CourtBooking.Infrastructure.Services.Scoring;

public class ScoringValidationService : IScoringValidationService
{
    public void ValidateCreateMatchRequest(CreateScoringMatchRequestDto request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.SportCode))
            errors.Add("Sport code is required.");

        if (string.IsNullOrWhiteSpace(request.RuleSetCode))
            errors.Add("Rule set code is required.");

        if (request.GameType != "Singles" && request.GameType != "Doubles")
            errors.Add("Game type must be 'Singles' or 'Doubles'.");

        if (request.MatchMode != "OpenPlay" && request.MatchMode != "Booking")
            errors.Add("Match mode must be 'OpenPlay' or 'Booking'.");

        if (request.Teams.Count != 2)
        {
            errors.Add("Exactly 2 teams are required.");
        }
        else
        {
            var teamCodes = request.Teams.Select(t => t.TeamCode).ToList();
            if (!teamCodes.Contains("A") || !teamCodes.Contains("B"))
                errors.Add("Team codes must be 'A' and 'B'.");

            var expectedPlayers = request.GameType == "Doubles" ? 2 : 1;

            foreach (var team in request.Teams)
            {
                if (team.Players.Count != expectedPlayers)
                {
                    errors.Add($"Team '{team.TeamCode}' must have exactly {expectedPlayers} player(s) for {request.GameType}.");
                }

                foreach (var player in team.Players)
                {
                    if (string.IsNullOrWhiteSpace(player.PlayerName))
                        errors.Add($"Player name is required for team '{team.TeamCode}'.");

                    if (!string.IsNullOrWhiteSpace(player.RegisteredUserId) && player.IsGuest)
                        errors.Add($"Player '{player.PlayerName}' cannot be both registered and guest.");

                    if (string.IsNullOrWhiteSpace(player.RegisteredUserId) && !player.IsGuest)
                        errors.Add($"Player '{player.PlayerName}' must be marked as guest if not registered.");
                }
            }
        }

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors));
    }
}
