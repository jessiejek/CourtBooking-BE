using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Domain.Entities.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CourtBooking.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<AuthResult> RegisterAsync(string fullName, string email, string password)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return new AuthResult { Success = false, Error = "Email is already registered." };
        }

        var user = new ApplicationUser
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return new AuthResult
            {
                Success = false,
                Error = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        await _userManager.AddToRoleAsync(user, "User");

        return await GenerateAuthResultAsync(user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return new AuthResult { Success = false, Error = "Invalid email or password." };
        }

        return await GenerateAuthResultAsync(user);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        var user = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
        if (user == null)
        {
            return new AuthResult { Success = false, Error = "Invalid or expired refresh token." };
        }

        return await GenerateAuthResultAsync(user);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "User",
            AvatarUrl = user.AvatarUrl
        };
    }

    private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user)
    {
        var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenDays = int.Parse(_configuration["JWT_REFRESH_TOKEN_DAYS"] ?? "7");
        var expiresAt = DateTime.UtcNow.AddMinutes(
            double.Parse(_configuration["JWT_ACCESS_TOKEN_MINUTES"] ?? "60"));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenDays);
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "User",
                AvatarUrl = user.AvatarUrl
            }
        };
    }
}
