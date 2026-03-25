namespace Todo.Core.Authentication;

public interface IJwtTokenService
{
    AccessTokenCreateResult CreateAccessToken(ValidatedUser user, IReadOnlySet<string> permissions);
}
