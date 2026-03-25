namespace Todo.Core.Authentication;

/// <summary>Resolved from the current HTTP context JWT principal.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Email { get; }

    UserRole? Role { get; }

    IReadOnlySet<string> Permissions { get; }
}
