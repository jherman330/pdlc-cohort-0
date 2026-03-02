using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Todo.Core.Common;
using Todo.Core.Exceptions;
using Serilog;

namespace Todo.Core.Middleware;

/// <summary>
/// Global error handling middleware. Catches exceptions, logs with Serilog, returns consistent error response per Backend blueprint.
/// </summary>
public class ErrorHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, code, message, errors) = MapException(ex);
        var correlationId = context.TraceIdentifier;
        var requestPath = context.Request.Path.Value ?? "";
        var userId = context.User?.Identity?.Name;

        Log.Error(ex, "Unhandled exception. {StatusCode} {Code} {CorrelationId} {RequestPath} {UserId}",
            (int)statusCode, code, correlationId, requestPath, userId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        object payload;
        if (errors != null)
        {
            payload = new { correlationId, errors };
        }
        else
        {
            var errorDetail = new { code, message, field = (string?)null };
            payload = new { correlationId, errors = new[] { errorDetail } };
        }

        if (string.Equals(_env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase) && errors == null)
        {
            var devError = new { code, message, field = (string?)null, detail = ex.ToString() };
            payload = new { correlationId, errors = new[] { devError } };
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static (HttpStatusCode statusCode, string code, string message, ApiError[]? errors) MapException(Exception ex)
    {
        switch (ex)
        {
            case ValidationException validation:
            {
                var errors = validation.Errors.SelectMany(kv => kv.Value.Select(msg => new ApiError
                {
                    Code = ErrorCodes.ValidationError,
                    Message = msg,
                    Field = kv.Key
                })).ToArray();
                return (HttpStatusCode.BadRequest, ErrorCodes.ValidationError, validation.Message, errors);
            }
            case ApplicationExceptionBase appEx:
                return (appEx.StatusCode, appEx.ErrorCode, appEx.Message, null);
            case KeyNotFoundException:
                return (HttpStatusCode.NotFound, ErrorCodes.NotFound, ex.Message, null);
            case ArgumentException:
                return (HttpStatusCode.BadRequest, ErrorCodes.BadRequest, ex.Message, null);
            case UnauthorizedAccessException:
                return (HttpStatusCode.Unauthorized, ErrorCodes.Unauthorized, ex.Message, null);
            default:
                return (HttpStatusCode.InternalServerError, ErrorCodes.InternalError, "An unexpected error occurred.", null);
        }
    }
}
