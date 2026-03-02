using Microsoft.AspNetCore.Mvc;

namespace Todo.Core.Services;

/// <summary>
/// Service for idempotency key handling: cache and retrieve responses by idempotency key (e.g., Redis-backed).
/// </summary>
public interface IIdempotencyService
{
    /// <summary>
    /// Returns a cached response for the given idempotency key if one exists; otherwise null.
    /// </summary>
    Task<IActionResult?> GetCachedResponseAsync(string idempotencyKey);

    /// <summary>
    /// Caches the response for the given idempotency key. Optional TTL; default from configuration.
    /// </summary>
    Task CacheResponseAsync(string idempotencyKey, IActionResult response, TimeSpan? ttl = null);

    /// <summary>
    /// Returns true if the key is valid (e.g., UUID v4 format).
    /// </summary>
    bool IsValidIdempotencyKey(string key);
}
