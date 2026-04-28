namespace AspireApp.E2E.Tests.Infrastructure;

public enum AuthStage
{
    NotStarted,
    NavigatingToApp,
    WaitingForClerkToLoad,
    ProviderSignIn,
    EnteringCredentials,
    AwaitingVerificationCode,
    EnteringVerificationCode,
    AppVerification,
    Completed,
    Failed,
}

public record AuthOrchestrationContext(
    string Email,
    string Password,
    string BaseUrl,
    string TestName)
{
    public AuthStage CurrentStage { get; set; } = AuthStage.NotStarted;
    public string? FailureReason { get; set; }
    public DateTimeOffset StartedAt { get; } = DateTimeOffset.UtcNow;
    public List<AuthStageTransition> Transitions { get; } = [];
}

public record AuthStageTransition(
    AuthStage From,
    AuthStage To,
    DateTimeOffset OccurredAt,
    string? Notes = null);
