using Microsoft.AspNetCore.Builder;

namespace Todo.Core.Middleware;

/// <summary>
/// Extension methods for registering middleware in the pipeline.
/// Recommended order: request logging → CORS → error handling → validation → routing.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>Adds request logging middleware.</summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();

    /// <summary>Adds global exception handling middleware.</summary>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ErrorHandlingMiddleware>();

    /// <summary>Adds validation middleware (pipeline slot). Model validation runs via ValidationFilter.</summary>
    public static IApplicationBuilder UseValidation(this IApplicationBuilder app)
        => app.UseMiddleware<ValidationMiddleware>();
}
