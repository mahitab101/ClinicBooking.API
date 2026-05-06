using System.Net;
using System.Text.Json;
using ClinicBooking.API.Common;

namespace ClinicBooking.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            InvalidOperationException   => (HttpStatusCode.BadRequest,          exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized,         exception.Message),
            KeyNotFoundException        => (HttpStatusCode.NotFound,             exception.Message),
            ArgumentException           => (HttpStatusCode.BadRequest,           exception.Message),
            _                           => (HttpStatusCode.InternalServerError,  "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.FailNoData(
            message: message,
            errors: _env.IsDevelopment()
                ? new List<string> { exception.ToString() }  // full stack trace in dev
                : null                                        // hide details in production
        );

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
