namespace AspireApp.E2E.Tests;

public class HealthCheckTests : BasePlaywrightTests
{
    [Test]
    public async Task Api_HealthCheck_ShouldReturn200()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{ApiBaseUrl}/health");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task Api_AliveCheck_ShouldReturn200()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{ApiBaseUrl}/alive");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task Api_Items_ShouldReturnPublicData()
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"{ApiBaseUrl}/api/items");
        response.IsSuccessStatusCode.Should().BeTrue();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Item One");
    }

    [Test]
    public async Task Web_ShouldBeReachable()
    {
        using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });
        var response = await httpClient.GetAsync(WebBaseUrl);
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
