using CourtBooking.Domain.Entities.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CourtBooking.Application.Common.Interfaces;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    Task<ApplicationUser?> ValidateRefreshTokenAsync(string refreshToken);
}
