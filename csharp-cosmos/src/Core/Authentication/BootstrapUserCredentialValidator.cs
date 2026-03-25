using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Todo.Core.Configuration;

namespace Todo.Core.Authentication;

/// <summary>
/// Validates bootstrap users using ASP.NET Identity password hashes.
/// Hash format must match <see cref="PasswordHasher{T}"/> with user discriminator <c>bootstrap</c>.
/// </summary>
public sealed class BootstrapUserCredentialValidator : IUserCredentialValidator
{
    internal const string PasswordHasherUser = "bootstrap";

    private readonly IOptionsMonitor<BootstrapAuthSettings> _options;
    private readonly PasswordHasher<string> _hasher = new();

    public BootstrapUserCredentialValidator(IOptionsMonitor<BootstrapAuthSettings> options) =>
        _options = options;

    public Task<ValidatedUser?> ValidateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var match = _options.CurrentValue.Users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        if (match == null)
            return Task.FromResult<ValidatedUser?>(null);

        if (_hasher.VerifyHashedPassword(PasswordHasherUser, match.PasswordHash, password) != PasswordVerificationResult.Success)
            return Task.FromResult<ValidatedUser?>(null);

        if (!Enum.TryParse<UserRole>(match.Role, ignoreCase: true, out var role))
            return Task.FromResult<ValidatedUser?>(null);

        return Task.FromResult<ValidatedUser?>(new ValidatedUser
        {
            UserId = match.UserId,
            Email = match.Email,
            Role = role,
        });
    }
}
