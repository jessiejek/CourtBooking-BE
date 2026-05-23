using System.Text.Json;
using System.Text.Json.Serialization;
using CourtBooking.Application.Common.Interfaces;
using CourtBooking.Domain.Entities.Authentication;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CourtBooking.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtService jwtService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
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

    public async Task<AuthResult> SocialLoginAsync(SocialLoginRequestDto request)
    {
        string email;
        string fullName;
        string? avatarUrl;

        switch (request.Provider.ToLower())
        {
            case "google":
            {
                if (string.IsNullOrWhiteSpace(request.IdToken))
                    return new AuthResult { Success = false, Error = "Google ID token is required." };

                var googleClientId = _configuration["GOOGLE_CLIENT_ID"]
                    ?? throw new InvalidOperationException("GOOGLE_CLIENT_ID is not configured.");

                GoogleJsonWebSignature.Payload payload;
                try
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                        new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { googleClientId } });
                }
                catch (InvalidJwtException)
                {
                    return new AuthResult { Success = false, Error = "Invalid Google token." };
                }

                email = payload.Email;
                fullName = payload.Name ?? payload.GivenName ?? email.Split('@')[0];
                avatarUrl = payload.Picture;
                break;
            }

            case "facebook":
            {
                if (string.IsNullOrWhiteSpace(request.AccessToken))
                    return new AuthResult { Success = false, Error = "Facebook access token is required." };

                var url = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={request.AccessToken}";
                HttpResponseMessage fbResponse;

                try
                {
                    fbResponse = await _httpClient.GetAsync(url);
                }
                catch
                {
                    return new AuthResult { Success = false, Error = "Failed to verify Facebook token." };
                }

                if (!fbResponse.IsSuccessStatusCode)
                    return new AuthResult { Success = false, Error = "Invalid Facebook access token." };

                var fbJson = await fbResponse.Content.ReadAsStringAsync();
                var fbUser = JsonSerializer.Deserialize<FacebookUserResult>(fbJson);

                if (fbUser == null || string.IsNullOrWhiteSpace(fbUser.Email))
                    return new AuthResult { Success = false, Error = "Facebook email permission is required." };

                email = fbUser.Email;
                fullName = fbUser.Name ?? email.Split('@')[0];
                avatarUrl = fbUser.Picture?.Data?.Url;
                break;
            }

            default:
                return new AuthResult { Success = false, Error = $"Unsupported provider '{request.Provider}'." };
        }

        if (string.IsNullOrWhiteSpace(email))
            return new AuthResult { Success = false, Error = "Could not retrieve email from provider." };

        // Find or create user
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                FullName = fullName,
                Email = email,
                UserName = email,
                AvatarUrl = avatarUrl,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Error = string.Join(", ", createResult.Errors.Select(e => e.Description))
                };
            }

            await _userManager.AddToRoleAsync(user, "User");
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

internal class FacebookUserResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("picture")]
    public FacebookPicture? Picture { get; set; }
}

internal class FacebookPicture
{
    [JsonPropertyName("data")]
    public FacebookPictureData? Data { get; set; }
}

internal class FacebookPictureData
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
