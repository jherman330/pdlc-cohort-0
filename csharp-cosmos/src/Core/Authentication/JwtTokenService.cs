using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Todo.Core.Configuration;

namespace Todo.Core.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IOptionsMonitor<JwtSettings> _options;

    public JwtTokenService(IOptionsMonitor<JwtSettings> options) => _options = options;

    public AccessTokenCreateResult CreateAccessToken(ValidatedUser user, IReadOnlySet<string> permissions)
    {
        var jwt = _options.CurrentValue;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.Email, user.Email),
            new(AuthClaimTypes.Role, user.Role.ToString()),
        };
        if (!string.IsNullOrEmpty(user.TenantId))
            claims.Add(new Claim(AuthClaimTypes.TenantId, user.TenantId));
        foreach (var permission in permissions)
            claims.Add(new Claim(AuthClaimTypes.Permission, permission));

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresIn = Math.Max(1, (int)(expires - DateTime.UtcNow).TotalSeconds);
        return new AccessTokenCreateResult(tokenString, expiresIn);
    }
}
