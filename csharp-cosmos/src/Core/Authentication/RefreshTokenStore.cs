using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Todo.Core.Configuration;

namespace Todo.Core.Authentication;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IDistributedCache _cache;
    private readonly IOptionsMonitor<JwtSettings> _jwt;

    public RefreshTokenStore(IDistributedCache cache, IOptionsMonitor<JwtSettings> jwt)
    {
        _cache = cache;
        _jwt = jwt;
    }

    public async Task StoreAsync(string refreshToken, ValidatedUser user, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(refreshToken);
        var payload = new RefreshTokenRecord
        {
            UserId = user.UserId,
            Email = user.Email,
            Role = user.Role.ToString(),
        };
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var opts = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(Math.Max(1, _jwt.CurrentValue.RefreshTokenDays)),
        };
        await _cache.SetStringAsync(key, json, opts, cancellationToken);
    }

    public async Task<RefreshTokenRecord?> GetAndRemoveAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(refreshToken);
        var json = await _cache.GetStringAsync(key, cancellationToken);
        await _cache.RemoveAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(json))
            return null;
        return JsonSerializer.Deserialize<RefreshTokenRecord>(json, JsonOptions);
    }

    public async Task<bool> TryRemoveForUserAsync(string refreshToken, string userId, CancellationToken cancellationToken = default)
    {
        var key = CacheKey(refreshToken);
        var json = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(json))
            return true;
        var record = JsonSerializer.Deserialize<RefreshTokenRecord>(json, JsonOptions);
        if (record == null || !string.Equals(record.UserId, userId, StringComparison.Ordinal))
            return false;
        await _cache.RemoveAsync(key, cancellationToken);
        return true;
    }

    private static string CacheKey(string token) =>
        "auth:refresh:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
