using Microsoft.Playwright;

namespace AspireApp.E2E.Tests.Infrastructure;

public class AuthOrchestrationCoordinator
{
    private readonly IPage _page;
    private readonly AuthOrchestrationContext _context;
    private readonly AuthDiagnosticsWriter _diagnostics;

    public AuthOrchestrationCoordinator(IPage page, AuthOrchestrationContext context, AuthDiagnosticsWriter diagnostics)
    {
        _page = page;
        _context = context;
        _diagnostics = diagnostics;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await TransitionToAsync(AuthStage.NavigatingToApp);

            // Navigate and wait for page to load
            await _page.GotoAsync(_context.BaseUrl);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Wait for Clerk to load (look for sign-in trigger)
            await TransitionToAsync(AuthStage.WaitingForClerkToLoad);
            await WaitForClerkAsync(cancellationToken);

            // Click sign-in
            await TransitionToAsync(AuthStage.ProviderSignIn);
            await ClickSignInAsync();

            // Wait for Clerk modal
            await _page.WaitForSelectorAsync(PageSelectors.ClerkModal, new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000,
            });

            // Enter credentials
            await TransitionToAsync(AuthStage.EnteringCredentials);
            await EnterEmailAsync();

            // Wait for OTP or password step
            await TransitionToAsync(AuthStage.AwaitingVerificationCode);
            await WaitForVerificationInputAsync();

            // Enter OTP code
            await TransitionToAsync(AuthStage.EnteringVerificationCode);
            await EnterVerificationCodeAsync("424242");

            // Wait for app to confirm auth
            await TransitionToAsync(AuthStage.AppVerification);
            var success = await WaitForAuthenticatedAsync(cancellationToken);

            if (success)
            {
                await TransitionToAsync(AuthStage.Completed);
                return true;
            }
            else
            {
                await TransitionToAsync(AuthStage.Failed, "Did not reach authenticated state");
                return false;
            }
        }
        catch (Exception ex)
        {
            _context.FailureReason = ex.Message;
            await _diagnostics.WriteDiagnosticsAsync(_page, _context, ex);
            return false;
        }
    }

    private async Task WaitForClerkAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        while (!cts.Token.IsCancellationRequested)
        {
            var trigger = await _page.QuerySelectorAsync(PageSelectors.LoginTrigger);
            if (trigger is not null) return;
            await Task.Delay(500, cts.Token);
        }

        throw new TimeoutException("Timed out waiting for Clerk sign-in trigger to appear.");
    }

    private async Task ClickSignInAsync()
    {
        var trigger = await _page.WaitForSelectorAsync(PageSelectors.LoginTrigger, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000,
        });
        await trigger!.ClickAsync();
    }

    private async Task EnterEmailAsync()
    {
        var emailInput = await _page.WaitForSelectorAsync(
            "input[type='email'], input[name='identifier'], .cl-signIn-root input[autocomplete='email']",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 15_000 });
        await emailInput!.FillAsync(_context.Email);
        await emailInput.PressAsync("Enter");
    }

    private async Task WaitForVerificationInputAsync()
    {
        await _page.WaitForSelectorAsync(
            "input[name='password'], input[autocomplete='one-time-code'], .cl-otpCodeField input",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 15_000 });
    }

    private async Task EnterVerificationCodeAsync(string code)
    {
        // Try password field first
        var passwordInput = await _page.QuerySelectorAsync("input[name='password']");
        if (passwordInput is not null)
        {
            await passwordInput.FillAsync(_context.Password);
            await passwordInput.PressAsync("Enter");
            return;
        }

        // Try OTP field
        var otpInputs = await _page.QuerySelectorAllAsync(".cl-otpCodeField input, input[autocomplete='one-time-code']");
        if (otpInputs.Count > 0)
        {
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
        }
    }

    private async Task<bool> WaitForAuthenticatedAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        while (!cts.Token.IsCancellationRequested)
        {
            var authElement = await _page.QuerySelectorAsync(PageSelectors.AuthenticatedUi);
            if (authElement is not null) return true;
            await Task.Delay(500, cts.Token);
        }

        return false;
    }

    private async Task TransitionToAsync(AuthStage newStage, string? notes = null)
    {
        var transition = new AuthStageTransition(_context.CurrentStage, newStage, DateTimeOffset.UtcNow, notes);
        _context.Transitions.Add(transition);
        _context.CurrentStage = newStage;
        await _diagnostics.WriteTransitionAsync(transition);
    }
}
