namespace Todo.Core.Authentication;

/// <summary>Resolved from the current HTTP context JWT principal.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Email { get; }

    /// <summary>Organization tenant id for database RLS (JWT claim <see cref="AuthClaimTypes.TenantId"/>).</summary>
    string? TenantId { get; }

    UserRole? Role { get; }

    IReadOnlySet<string> Permissions { get; }
}
