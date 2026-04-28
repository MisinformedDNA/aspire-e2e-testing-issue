using Microsoft.Playwright;

namespace AspireApp.E2E.Tests.Infrastructure;

public sealed class PlaywrightManager : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not started.");

    public async Task StartAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-dev-shm-usage"],
        });
    }

    public async Task<IBrowserContext> CreateContextAsync(string? storageStatePath = null)
    {
        if (_browser is null) throw new InvalidOperationException("Browser not started.");

        var contextOptions = new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        };

        if (storageStatePath is not null && File.Exists(storageStatePath))
        {
            contextOptions.StorageStatePath = storageStatePath;
        }

        return await _browser.NewContextAsync(contextOptions);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }
}
