namespace Todo.Core.Authentication;

/// <summary>Authenticated principal after credential validation.</summary>
public sealed class ValidatedUser
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    /// <summary>Organization tenant id for RLS; omitted when not using multi-tenant bootstrap users.</summary>
    public string? TenantId { get; init; }

    public UserRole Role { get; init; }
}
