using Microsoft.Playwright;

namespace AspireApp.E2E.Tests.Infrastructure;

public class AuthReadinessGateEvaluator
{
    private readonly IPage _page;

    public AuthReadinessGateEvaluator(IPage page)
    {
        _page = page;
    }

    public async Task<bool> IsClerkLoadedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _page.EvaluateAsync<bool>(
                "() => typeof window.Clerk !== 'undefined' && window.Clerk !== null");
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var trigger = await _page.QuerySelectorAsync(PageSelectors.AuthenticatedUi);
            return trigger is not null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> WaitForAuthReadinessAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        while (!cts.Token.IsCancellationRequested)
        {
            if (await IsClerkLoadedAsync(cts.Token))
                return true;
            await Task.Delay(500, cts.Token).ConfigureAwait(false);
        }

        return false;
    }
}
