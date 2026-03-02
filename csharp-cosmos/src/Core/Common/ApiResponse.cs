namespace Todo.Core.Common;

/// <summary>
/// Standard API success response with data. Used for GET/single-resource and other success payloads.
/// </summary>
/// <typeparam name="T">Type of the response data.</typeparam>
public sealed class ApiResponse<T>
{
    public T Data { get; init; } = default!;
}

/// <summary>
/// Standard API success response without data. Used for 200 OK with no body or simple acknowledgments.
/// </summary>
public sealed class ApiResponse
{
    public static readonly ApiResponse Success = new();
}

/// <summary>
/// Standard API error response per Backend blueprint: errors array with code, message, and optional field.
/// </summary>
public sealed class ApiErrorResponse
{
    public ApiError[] Errors { get; init; } = Array.Empty<ApiError>();
}

/// <summary>
/// Single error entry in an API error response.
/// </summary>
public sealed class ApiError
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Field { get; init; }
}
