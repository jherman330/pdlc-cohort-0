using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Todo.Core.Services;

/// <summary>
/// Redis-backed idempotency key handling. Caches response status and body with configurable TTL.
/// </summary>
public sealed class IdempotencyService : IIdempotencyService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly IDistributedCache _cache;
    private readonly IdempotencyOptions _options;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(
        IDistributedCache cache,
        IOptions<IdempotencyOptions> options,
        ILogger<IdempotencyService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IActionResult?> GetCachedResponseAsync(string idempotencyKey)
    {
        if (!_options.Enabled)
            return null;

        var key = BuildCacheKey(idempotencyKey);
        var bytes = await _cache.GetAsync(key);
        if (bytes == null || bytes.Length == 0)
        {
            Log.Debug("Idempotency cache miss for key {Key}", key);
            return null;
        }

        Log.Debug("Idempotency cache hit for key {Key}", key);
        var json = Encoding.UTF8.GetString(bytes);
        var dto = JsonSerializer.Deserialize<CachedResponseDto>(json);
        if (dto == null)
            return null;

        return new ContentResult
        {
            StatusCode = dto.StatusCode,
            ContentType = dto.ContentType ?? "application/json",
            Content = dto.Body
        };
    }

    /// <inheritdoc />
    public async Task CacheResponseAsync(string idempotencyKey, IActionResult response, TimeSpan? ttl = null)
    {
        if (!_options.Enabled)
            return;

        if (response is not ObjectResult objectResult || objectResult.Value == null)
        {
            _logger.LogDebug("Idempotency cache skip: unsupported result type or null value");
            return;
        }

        var statusCode = objectResult.StatusCode ?? 200;
        var body = JsonSerializer.Serialize(objectResult.Value, JsonOptions);
        var dto = new CachedResponseDto { StatusCode = statusCode, ContentType = "application/json", Body = body };
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var key = BuildCacheKey(idempotencyKey);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl ?? _options.Ttl };

        await _cache.SetAsync(key, bytes, options);
    }

    /// <inheritdoc />
    public bool IsValidIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;
        return Guid.TryParseExact(key, "D", out var guid) && guid != default && IsUuidV4(guid);
    }

    private string BuildCacheKey(string idempotencyKey) => $"{_options.KeyPrefix}{idempotencyKey}";

    private static bool IsUuidV4(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return (bytes[6] & 0xF0) == 0x40 && (bytes[8] & 0xC0) == 0x80;
    }

    private sealed class CachedResponseDto
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public string? Body { get; set; }
    }
}

/// <summary>
/// Configuration for idempotency cache (TTL, key prefix, enabled). Binds to IdempotencyCache in appsettings.
/// </summary>
public sealed class IdempotencyOptions
{
    public const string SectionName = "IdempotencyCache";
    public bool Enabled { get; set; } = true;
    public int TtlHours { get; set; } = 24;
    public string KeyPrefix { get; set; } = "idempotency:";
    public TimeSpan Ttl => TimeSpan.FromHours(TtlHours);
}
