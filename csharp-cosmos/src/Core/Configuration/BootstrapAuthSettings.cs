namespace Todo.Core.Configuration;

/// <summary>
/// Bootstrap users for login without external IdP (WO-2). Passwords are stored as ASP.NET Identity PasswordHasher v3 hashes.
/// Hashes in repo are generated with PasswordHasher discriminator <c>bootstrap</c>.
/// </summary>
public sealed class BootstrapAuthSettings
{
    public const string SectionName = "BootstrapAuth";

    public List<BootstrapUserOptions> Users { get; set; } = new();
}

/// <summary>Single bootstrap account.</summary>
public sealed class BootstrapUserOptions
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
