using Microsoft.Playwright;

namespace AspireApp.E2E.Tests.Infrastructure;

public class AuthDiagnosticsWriter
{
    private readonly string _testName;

    public AuthDiagnosticsWriter(string testName)
    {
        _testName = testName;
    }

    public Task WriteTransitionAsync(AuthStageTransition transition)
    {
        Console.WriteLine(
            $"[Auth:{_testName}] {transition.OccurredAt:HH:mm:ss.fff} " +
            $"{transition.From} → {transition.To}" +
            (transition.Notes is not null ? $" ({transition.Notes})" : ""));
        return Task.CompletedTask;
    }

    public async Task WriteDiagnosticsAsync(IPage page, AuthOrchestrationContext context, Exception? exception = null)
    {
        Console.WriteLine($"[Auth:{_testName}] Auth diagnostics at stage {context.CurrentStage}:");
        Console.WriteLine($"  URL: {page.Url}");
        Console.WriteLine($"  Started: {context.StartedAt:HH:mm:ss}");
        Console.WriteLine($"  Elapsed: {DateTimeOffset.UtcNow - context.StartedAt:g}");

        if (exception is not null)
            Console.WriteLine($"  Exception: {exception.Message}");

        if (context.FailureReason is not null)
            Console.WriteLine($"  Failure: {context.FailureReason}");

        foreach (var t in context.Transitions)
        {
            Console.WriteLine($"  {t.OccurredAt:HH:mm:ss.fff} {t.From} → {t.To}" +
                              (t.Notes is not null ? $" ({t.Notes})" : ""));
        }

        try
        {
            var content = await page.ContentAsync();
            if (content.Length > 500)
                Console.WriteLine($"  Page content (first 500 chars): {content[..500]}");
        }
        catch
        {
            // ignore
        }
    }
}
