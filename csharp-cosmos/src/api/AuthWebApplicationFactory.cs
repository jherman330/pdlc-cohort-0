using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Todo.Api.Tests;

/// <summary>Web application factory with JWT and bootstrap users for integration tests.</summary>
public class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    internal const string ItManagerEmail = "it.manager@local.test";
    internal const string ExecutiveEmail = "exec@local.test";
    internal const string TestPassword = "TestPassword123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Issuer", "test-issuer");
        builder.UseSetting("Jwt:Audience", "test-audience");
        builder.UseSetting("Jwt:SigningKey", "unit_test_jwt_signing_key_min_32_chars__");
        builder.UseSetting("Jwt:AccessTokenMinutes", "60");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");
        builder.UseSetting("BootstrapAuth:Users:0:UserId", "a0000000-0000-4000-8000-000000000001");
        builder.UseSetting("BootstrapAuth:Users:0:Email", ItManagerEmail);
        builder.UseSetting(
            "BootstrapAuth:Users:0:PasswordHash",
            "AQAAAAIAAYagAAAAEN9jQFR6FQCOZbYizpTogkRJBwF86TAmCFBuf7yn8smwVeH0GKwBRifSIc1G7Kd0wQ==");
        builder.UseSetting("BootstrapAuth:Users:0:Role", "ItManager");
        builder.UseSetting("BootstrapAuth:Users:0:TenantId", "test-tenant-001");
        builder.UseSetting("BootstrapAuth:Users:1:UserId", "b0000000-0000-4000-8000-000000000002");
        builder.UseSetting("BootstrapAuth:Users:1:Email", ExecutiveEmail);
        builder.UseSetting(
            "BootstrapAuth:Users:1:PasswordHash",
            "AQAAAAIAAYagAAAAEN9jQFR6FQCOZbYizpTogkRJBwF86TAmCFBuf7yn8smwVeH0GKwBRifSIc1G7Kd0wQ==");
        builder.UseSetting("BootstrapAuth:Users:1:Role", "Executive");
        builder.UseSetting("BootstrapAuth:Users:1:TenantId", "test-tenant-002");
    }
}
