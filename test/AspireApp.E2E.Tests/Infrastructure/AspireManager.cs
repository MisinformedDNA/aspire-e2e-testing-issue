using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AspireApp.E2E.Tests.Infrastructure;

public sealed class AspireManager : IAsyncDisposable
{
    private DistributedApplication? _app;

    public DistributedApplication App => _app ?? throw new InvalidOperationException("Aspire app not started.");

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var timeoutSeconds = int.TryParse(
            Environment.GetEnvironmentVariable("ASPIREAPP_E2E_ASPIRE_START_TIMEOUT_SECONDS"),
            out var parsed) ? parsed : 120;

        var appBuilder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.AspireApp_AppHost>([], cancellationToken);

        appBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appBuilder.BuildAsync(cancellationToken);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        await _app.StartAsync(linkedCts.Token);
    }

    public async Task WaitForResourceReadyAsync(string resourceName, CancellationToken cancellationToken = default)
    {
        if (_app is null) throw new InvalidOperationException("Aspire app not started.");

        var rns = _app.Services.GetRequiredService<ResourceNotificationService>();
        await rns.WaitForResourceAsync(resourceName, cancellationToken: cancellationToken);
    }

    public string GetEndpoint(string resourceName, string endpointName = "http")
    {
        if (_app is null) throw new InvalidOperationException("Aspire app not started.");
        return _app.GetEndpoint(resourceName, endpointName).ToString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}
