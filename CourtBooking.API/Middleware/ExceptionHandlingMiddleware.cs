using System.Net;
using System.Text.Json;
using CourtBooking.Application.Common.Models;

namespace CourtBooking.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";

        if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Forbidden;
            message = "You do not have permission to perform this action.";
        }

        if (exception is KeyNotFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;
            message = exception.Message;
        }

        if (exception is ArgumentException || exception is InvalidOperationException)
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            message = exception.Message;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiErrorResponse
        {
            Message = message,
            StatusCode = statusCode
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
