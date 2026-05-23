namespace CourtBooking.Application.Common.Interfaces;

public class SocialLoginRequestDto
{
    public string Provider { get; set; } = string.Empty; // "google" or "facebook"
    public string? IdToken { get; set; }
    public string? AccessToken { get; set; }
}
