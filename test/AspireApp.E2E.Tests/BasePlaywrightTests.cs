using Microsoft.Playwright;

namespace AspireApp.E2E.Tests;

public abstract class BasePlaywrightTests
{
    private const string ApiResourceName = "aspireapp-api";
    private const string WebResourceName = "aspireapp-web";

    protected static AspireManager AspireManager { get; } = new();
    protected static PlaywrightManager PlaywrightManager { get; } = new();

    protected static string WebBaseUrl { get; private set; } = string.Empty;
    protected static string ApiBaseUrl { get; private set; } = string.Empty;

    // Stub credentials — no secrets needed
    private const string StubEmail = "test@example.com";
    private const string StubPassword = "password";

    [Before(Class)]
    public static async Task StartInfrastructureAsync()
    {
        await AspireManager.StartAsync();

        await AspireManager.WaitForResourceReadyAsync(ApiResourceName);
        await AspireManager.WaitForResourceReadyAsync(WebResourceName);

        WebBaseUrl = AspireManager.GetEndpoint(WebResourceName, "http");
        ApiBaseUrl = AspireManager.GetEndpoint(ApiResourceName, "http");

        await PlaywrightManager.StartAsync();

        // Pre-warm the web app
        await PreWarmWebAppAsync();
    }

    [After(Class)]
    public static async Task StopInfrastructureAsync()
    {
        await PlaywrightManager.DisposeAsync();
        await AspireManager.DisposeAsync();
    }

    private static async Task PreWarmWebAppAsync()
    {
        await using var context = await PlaywrightManager.CreateContextAsync();
        var page = await context.NewPageAsync();
        try
        {
            await page.GotoAsync(WebBaseUrl);
            await page.WaitForSelectorAsync(PageSelectors.NavBrand, new PageWaitForSelectorOptions
            {
                Timeout = 30_000,
                State = WaitForSelectorState.Visible,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PreWarm] Warning: {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    protected static async Task<IBrowserContext> CreateBrowserContextAsync(string? storageStatePath = null)
    {
        return await PlaywrightManager.CreateContextAsync(storageStatePath);
    }

    protected static async Task<bool> AuthenticateUserAsync(
        IPage page,
        string? email = null,
        string? password = null)
    {
        return await CompleteStubAuthenticationAsync(
            page,
            email ?? StubEmail,
            password ?? StubPassword);
    }

    protected static async Task<bool> CompleteStubAuthenticationAsync(
        IPage page,
        string email,
        string password)
    {
        try
        {
            await page.GotoAsync($"{WebBaseUrl}/login");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.FillAsync("#login-email", email);
            await page.FillAsync("#login-password", password);
            await page.ClickAsync("#login-submit");

            await page.WaitForURLAsync(url => !url.Contains("/login"), new PageWaitForURLOptions
            {
                Timeout = 15_000,
            });

            var authUi = await page.QuerySelectorAsync(PageSelectors.AuthenticatedUi);
            return authUi is not null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Auth] Stub auth failed: {ex.Message}");
            return false;
        }
    }
}
