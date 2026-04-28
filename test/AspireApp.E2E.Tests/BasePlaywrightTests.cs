using Aspire.Hosting.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace AspireApp.E2E.Tests;

public abstract class BasePlaywrightTests
{
    private const string ApiResourceName = "aspireapp-api";
    private const string WebResourceName = "aspireapp-web";
    private const string CosmosDbResourceName = "cosmosdb";

    protected static AspireManager AspireManager { get; } = new();
    protected static PlaywrightManager PlaywrightManager { get; } = new();

    protected static string WebBaseUrl { get; private set; } = string.Empty;
    protected static string ApiBaseUrl { get; private set; } = string.Empty;

    [Before(Class)]
    public static async Task StartInfrastructureAsync()
    {
        await AspireManager.StartAsync();

        await AspireManager.WaitForResourceReadyAsync(CosmosDbResourceName);
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
        var testEmail = email
            ?? Environment.GetEnvironmentVariable("E2E_TEST_EMAIL")
            ?? TestConfig.Email;
        var testPassword = password
            ?? Environment.GetEnvironmentVariable("E2E_TEST_PASSWORD")
            ?? TestConfig.Password;

        return await CompleteClerkAuthenticationAsync(page, testEmail, testPassword);
    }

    protected static async Task<bool> CompleteClerkAuthenticationAsync(
        IPage page,
        string email,
        string password)
    {
        var context = new AuthOrchestrationContext(email, password, WebBaseUrl, "CompleteClerkAuth");
        var diagnostics = new AuthDiagnosticsWriter("CompleteClerkAuth");
        var coordinator = new AuthOrchestrationCoordinator(page, context, diagnostics);

        return await coordinator.ExecuteAsync();
    }

    protected static async Task WaitForClerkToLoadAsync(IPage page, int timeoutMs = 15_000)
    {
        var evaluator = new AuthReadinessGateEvaluator(page);
        await evaluator.WaitForAuthReadinessAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }

    protected static async Task<bool> EnterClerkVerificationCodeAsync(IPage page, string code = "424242")
    {
        try
        {
            var otpInputs = await page.QuerySelectorAllAsync(
                ".cl-otpCodeField input, input[autocomplete='one-time-code']");

            if (otpInputs.Count == 0) return false;

            if (otpInputs.Count == 1)
            {
                await otpInputs[0].FillAsync(code);
                await otpInputs[0].PressAsync("Enter");
            }
            else
            {
                for (var i = 0; i < Math.Min(code.Length, otpInputs.Count); i++)
                {
                    await otpInputs[i].TypeAsync(code[i].ToString());
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Test configuration - loaded from user-secrets or environment variables.
/// </summary>
internal static class TestConfig
{
    private static IConfiguration? _config;

    private static IConfiguration Config => _config ??= new ConfigurationBuilder()
        .AddUserSecrets("aspireapp-e2e-tests-secrets")
        .AddEnvironmentVariables()
        .Build();

    public static string Email => Config["TestEmail"] ?? "test3+clerk_test@test.com";
    public static string Password => Config["TestPassword"] ?? string.Empty;
}
