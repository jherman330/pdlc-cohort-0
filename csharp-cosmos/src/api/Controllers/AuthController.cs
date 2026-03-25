using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Core.Authentication;
using Todo.Core.Common;

namespace Todo.Api.Controllers;

/// <summary>JWT login, refresh, and logout (WO-2).</summary>
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Exchange bootstrap credentials for access and refresh tokens.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.LoginAsync(request.Email, request.Password, cancellationToken);
        return Ok(result);
    }

    /// <summary>Obtain a new token pair using a refresh token (rotation).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _auth.RefreshAsync(request.RefreshToken, cancellationToken);
        return Ok(result);
    }

    /// <summary>Revoke a refresh token for the authenticated user.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ApiErrorResponse { Errors = new[] { new ApiError { Code = ErrorCodes.Unauthorized, Message = "Not authenticated." } } });

        await _auth.LogoutAsync(request.RefreshToken, userId, cancellationToken);
        return NoContent();
    }
}
