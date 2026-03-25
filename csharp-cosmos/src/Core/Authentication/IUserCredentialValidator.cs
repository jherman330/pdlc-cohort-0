namespace Todo.Core.Authentication;

public interface IUserCredentialValidator
{
    Task<ValidatedUser?> ValidateAsync(string email, string password, CancellationToken cancellationToken = default);
}
