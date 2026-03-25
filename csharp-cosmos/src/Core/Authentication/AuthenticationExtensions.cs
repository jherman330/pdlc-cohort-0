using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Todo.Core.Configuration;

namespace Todo.Core.Authentication;

public static class AuthenticationExtensions
{
    /// <summary>Registers JWT bearer authentication, permission policies, bootstrap auth, and refresh token storage.</summary>
    public static IServiceCollection AddJwtAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<BootstrapAuthSettings>(configuration.GetSection(BootstrapAuthSettings.SectionName));

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrEmpty(jwt.SigningKey) || jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey is missing or shorter than 32 characters. Configure Jwt:SigningKey (environment, Key Vault, or User Secrets).");
        }

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, JsonAuthorizationMiddlewareResultHandler>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IRefreshTokenStore, RefreshTokenStore>();
        services.AddSingleton<IUserCredentialValidator, BootstrapUserCredentialValidator>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT short claim names (sub, permission) so policies and logout match emitted tokens.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = AuthClaimTypes.Role,
                };
            });

        services.AddAuthorization(options =>
        {
            foreach (var permission in RolePermissionRegistry.AllPermissionValues)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireClaim(AuthClaimTypes.Permission, permission));
            }
        });

        return services;
    }
}
