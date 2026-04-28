namespace AspireApp.E2E.Tests.Infrastructure;

public class AuthStageTimeoutPolicy
{
    private static readonly Dictionary<AuthStage, TimeSpan> DefaultTimeouts = new()
    {
        [AuthStage.NotStarted] = TimeSpan.FromSeconds(5),
        [AuthStage.NavigatingToApp] = TimeSpan.FromSeconds(30),
        [AuthStage.WaitingForClerkToLoad] = TimeSpan.FromSeconds(30),
        [AuthStage.ProviderSignIn] = TimeSpan.FromSeconds(15),
        [AuthStage.EnteringCredentials] = TimeSpan.FromSeconds(15),
        [AuthStage.AwaitingVerificationCode] = TimeSpan.FromSeconds(15),
        [AuthStage.EnteringVerificationCode] = TimeSpan.FromSeconds(15),
        [AuthStage.AppVerification] = TimeSpan.FromSeconds(30),
        [AuthStage.Completed] = TimeSpan.FromSeconds(5),
        [AuthStage.Failed] = TimeSpan.FromSeconds(5),
    };

    public TimeSpan GetTimeout(AuthStage stage)
    {
        return DefaultTimeouts.TryGetValue(stage, out var timeout)
            ? timeout
            : TimeSpan.FromSeconds(30);
    }
}
