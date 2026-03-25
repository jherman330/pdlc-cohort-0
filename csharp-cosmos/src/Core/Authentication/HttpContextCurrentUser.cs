using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Todo.Core.Authentication;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HttpContextCurrentUser(IHttpContextAccessor http) => _http = http;

    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? UserId => _http.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    public string? Email =>
        _http.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? _http.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    public UserRole? Role
    {
        get
        {
            var r = _http.HttpContext?.User.FindFirst(AuthClaimTypes.Role)?.Value;
            return r != null && Enum.TryParse<UserRole>(r, ignoreCase: true, out var role) ? role : null;
        }
    }

    public IReadOnlySet<string> Permissions
    {
        get
        {
            var user = _http.HttpContext?.User;
            if (user == null)
                return new HashSet<string>(StringComparer.Ordinal);
            return user.FindAll(AuthClaimTypes.Permission).Select(c => c.Value).ToHashSet(StringComparer.Ordinal);
        }
    }
}
