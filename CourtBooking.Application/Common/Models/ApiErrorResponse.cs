namespace CourtBooking.Application.Common.Models;

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
