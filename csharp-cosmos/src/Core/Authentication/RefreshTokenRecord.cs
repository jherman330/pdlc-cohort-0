namespace Todo.Core.Authentication;

/// <summary>Payload stored for an active refresh token (distributed cache).</summary>
public sealed class RefreshTokenRecord
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    /// <summary>Optional tenant id for RLS; omitted when not used.</summary>
    public string? TenantId { get; set; }
}
