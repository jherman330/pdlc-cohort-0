namespace Todo.Core.Common;

/// <summary>
/// Centralized error codes for API error responses. Use UPPER_SNAKE_CASE per Backend blueprint.
/// </summary>
public static class ErrorCodes
{
    public const string AssetNotFound = "ASSET_NOT_FOUND";
    public const string LicenseNotFound = "LICENSE_NOT_FOUND";
    public const string InvalidRole = "INVALID_ROLE";
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";
}
