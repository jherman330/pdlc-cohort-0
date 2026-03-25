namespace Todo.Core.Authentication;

public sealed class AccessTokenCreateResult
{
    public AccessTokenCreateResult(string token, int expiresInSeconds)
    {
        Token = token;
        ExpiresInSeconds = expiresInSeconds;
    }

    public string Token { get; }
    public int ExpiresInSeconds { get; }
}
