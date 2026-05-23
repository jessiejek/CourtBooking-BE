namespace CourtBooking.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string fullName, string email, string password);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> SocialLoginAsync(SocialLoginRequestDto request);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string userId);
    Task<UserDto?> GetCurrentUserAsync(string userId);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
    public string? Error { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? AvatarUrl { get; set; }
}
