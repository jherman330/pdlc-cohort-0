using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Todo.Api.Tests;

public class HealthEndpointTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly AuthWebApplicationFactory _factory;

    public HealthEndpointTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_Returns200AndHealthy()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task GetPingV1_Returns200AndPong()
    {
        var client = _factory.CreateClient();
        using var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AuthWebApplicationFactory.ItManagerEmail,
            password = AuthWebApplicationFactory.TestPassword,
        });
        loginResponse.EnsureSuccessStatusCode();
        await using var stream = await loginResponse.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var accessToken = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        Assert.NotNull(accessToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.GetAsync("/api/v1/ping");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("pong", body);
    }

    [Fact]
    public async Task GetRoot_Returns200()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Response_IncludesCorrelationIdHeader()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values) && values.Any());
    }
}
