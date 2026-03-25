using System.ComponentModel.DataAnnotations;

namespace Todo.Core.Authentication;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Password { get; set; } = string.Empty;
}

public sealed class RefreshRequest
{
    [Required]
    [MinLength(1)]
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutRequest
{
    [Required]
    [MinLength(1)]
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class AuthTokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresInSeconds { get; init; }
}
