using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Todo.Api.Tests;

public class AuthenticationTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly AuthWebApplicationFactory _factory;

    public AuthenticationTests(AuthWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_InvalidPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthWebApplicationFactory.ItManagerEmail,
            password = "wrong-password",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Valid_ReturnsTokens()
    {
        var client = _factory.CreateClient();
        var (access, refresh, _) = await LoginAsync(client, AuthWebApplicationFactory.ItManagerEmail, AuthWebApplicationFactory.TestPassword);
        Assert.False(string.IsNullOrEmpty(access));
        Assert.False(string.IsNullOrEmpty(refresh));
    }

    [Fact]
    public async Task Ping_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/ping");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ping_WithAccessToken_Returns200()
    {
        var client = _factory.CreateClient();
        var (access, _, _) = await LoginAsync(client, AuthWebApplicationFactory.ItManagerEmail, AuthWebApplicationFactory.TestPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
        var response = await client.GetAsync("/api/v1/ping");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("pong", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RbacProbe_Executive_Returns403()
    {
        var client = _factory.CreateClient();
        var (access, _, _) = await LoginAsync(client, AuthWebApplicationFactory.ExecutiveEmail, AuthWebApplicationFactory.TestPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
        var response = await client.GetAsync("/api/v1/rbac-probe");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusesConsumedToken_Returns401()
    {
        var client = _factory.CreateClient();
        var (_, refresh, _) = await LoginAsync(client, AuthWebApplicationFactory.ItManagerEmail, AuthWebApplicationFactory.TestPassword);

        var refresh1 = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = refresh });
        refresh1.EnsureSuccessStatusCode();

        var refresh2 = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = refresh });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh2.StatusCode);
    }

    [Fact]
    public async Task Logout_ThenRefresh_Returns401()
    {
        var client = _factory.CreateClient();
        var (access, refresh, _) = await LoginAsync(client, AuthWebApplicationFactory.ItManagerEmail, AuthWebApplicationFactory.TestPassword);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access);
        var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = refresh });
        logout.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;
        var refreshResponse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = refresh });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    private static async Task<(string AccessToken, string RefreshToken, int ExpiresIn)> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var data = doc.RootElement.GetProperty("data");
        return (
            data.GetProperty("accessToken").GetString() ?? "",
            data.GetProperty("refreshToken").GetString() ?? "",
            data.GetProperty("expiresInSeconds").GetInt32());
    }
}
