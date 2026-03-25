using Microsoft.AspNetCore.Mvc;

namespace Todo.Core.Common;

/// <summary>
/// Abstract base controller with common behavior: standard response patterns and Idempotency-Key support.
/// User context is available via <see cref="Authentication.ICurrentUser"/> (JWT claims).
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the Idempotency-Key header value if present (Backend blueprint).
    /// </summary>
    protected string? IdempotencyKey =>
        Request.Headers.TryGetValue("Idempotency-Key", out var value) ? value.ToString() : null;

    /// <summary>
    /// Returns 200 OK with standardized success payload: { "data": data }.
    /// </summary>
    protected IActionResult Ok<T>(T data)
    {
        return base.Ok(new ApiResponse<T> { Data = data });
    }

    /// <summary>
    /// Returns 201 Created with Location header and standardized body: { "data": data }.
    /// </summary>
    protected IActionResult Created<T>(string location, T data)
    {
        return base.Created(location, new ApiResponse<T> { Data = data });
    }

    /// <summary>
    /// Returns 400 Bad Request with blueprint-style validation errors (field-specific).
    /// </summary>
    protected IActionResult ValidationError(IReadOnlyDictionary<string, string[]> errors)
    {
        var apiErrors = errors.SelectMany(kv => kv.Value.Select(msg => new ApiError
        {
            Code = ErrorCodes.ValidationError,
            Message = msg,
            Field = kv.Key
        })).ToArray();
        return BadRequest(new ApiErrorResponse { Errors = apiErrors });
    }

    /// <summary>
    /// Returns a consistent error response shape per Backend blueprint: errors[] with code, message, field.
    /// </summary>
    protected IActionResult Error(string code, string message, string? field = null)
    {
        var error = new ApiError { Code = code, Message = message, Field = field };
        return BadRequest(new ApiErrorResponse { Errors = new[] { error } });
    }

    /// <summary>
    /// Returns 404 with blueprint-style error body.
    /// </summary>
    protected IActionResult NotFoundError(string code, string message)
    {
        var error = new ApiError { Code = code, Message = message, Field = null };
        return NotFound(new ApiErrorResponse { Errors = new[] { error } });
    }
}
