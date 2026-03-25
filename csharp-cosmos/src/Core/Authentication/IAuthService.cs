namespace Todo.Core.Authentication;

public interface IAuthService
{
    Task<AuthTokenResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AuthTokenResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task LogoutAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);
}
