namespace Todo.Core.Authentication;

/// <summary>Authenticated principal after credential validation.</summary>
public sealed class ValidatedUser
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
}
