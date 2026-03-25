using System.Security.Cryptography;
using Todo.Core.Common;
using Todo.Core.Exceptions;

namespace Todo.Core.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly IUserCredentialValidator _credentialValidator;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refreshStore;

    public AuthService(
        IUserCredentialValidator credentialValidator,
        IJwtTokenService jwt,
        IRefreshTokenStore refreshStore)
    {
        _credentialValidator = credentialValidator;
        _jwt = jwt;
        _refreshStore = refreshStore;
    }

    public async Task<AuthTokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _credentialValidator.ValidateAsync(email, password, cancellationToken);
        if (user == null)
            throw new AuthFailureException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var record = await _refreshStore.GetAndRemoveAsync(refreshToken, cancellationToken);
        if (record == null || !Enum.TryParse<UserRole>(record.Role, ignoreCase: true, out var role))
            throw new AuthFailureException(ErrorCodes.InvalidRefreshToken, "Refresh token is invalid or expired.");

        var user = new ValidatedUser
        {
            UserId = record.UserId,
            Email = record.Email,
            Role = role,
        };
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, string userId, CancellationToken cancellationToken = default)
    {
        var ok = await _refreshStore.TryRemoveForUserAsync(refreshToken, userId, cancellationToken);
        if (!ok)
            throw new ForbiddenOperationException("Refresh token does not belong to the current user.");
    }

    private async Task<AuthTokenResponse> IssueTokensAsync(ValidatedUser user, CancellationToken cancellationToken)
    {
        var permissions = RolePermissionRegistry.GetPermissions(user.Role);
        var access = _jwt.CreateAccessToken(user, permissions);
        var refresh = CreateRefreshTokenValue();
        await _refreshStore.StoreAsync(refresh, user, cancellationToken);
        return new AuthTokenResponse
        {
            AccessToken = access.Token,
            RefreshToken = refresh,
            ExpiresInSeconds = access.ExpiresInSeconds,
        };
    }

    private static string CreateRefreshTokenValue()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
