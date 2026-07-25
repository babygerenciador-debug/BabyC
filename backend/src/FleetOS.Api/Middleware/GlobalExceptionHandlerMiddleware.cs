using FleetOS.Shared.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FleetOS.Api.Middleware;

/// <summary>
/// Global exception handler that catches unhandled exceptions and returns
/// a standardized ProblemDetails JSON response instead of exposing stack traces.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware(
    ILogger<GlobalExceptionHandlerMiddleware> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        logger.LogError(
            exception,
            "Unhandled exception. CorrelationId: {CorrelationId} | Path: {Path}",
            correlationId,
            httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            ArgumentNullException       => (StatusCodes.Status400BadRequest,  "Bad Request"),
            ArgumentException           => (StatusCodes.Status400BadRequest,  "Bad Request"),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,    "Not Found"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,   "Forbidden"),
            InvalidOperationException   => (StatusCodes.Status409Conflict,    "Conflict"),
            OperationCanceledException  => (StatusCodes.Status408RequestTimeout, "Request Timeout"),
            NpgsqlException             => (StatusCodes.Status500InternalServerError, "Database Error"),
            _                           => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        var isDevelopment = httpContext.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Type     = $"https://httpstatuses.io/{statusCode}",
            Detail   = isDevelopment ? exception.Message : "An unexpected error occurred. Please try again later.",
            Instance = httpContext.Request.Path,
        };

        problem.Extensions["correlationId"] = correlationId;

        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
