namespace Todo.Core.Authentication;

public interface IRefreshTokenStore
{
    Task StoreAsync(string refreshToken, ValidatedUser user, CancellationToken cancellationToken = default);

    /// <summary>Reads and deletes the refresh token entry (rotation / single use).</summary>
    Task<RefreshTokenRecord?> GetAndRemoveAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Removes the token if it belongs to <paramref name="userId"/>.</summary>
    /// <returns><c>false</c> if the token exists but belongs to another user.</returns>
    Task<bool> TryRemoveForUserAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);
}
