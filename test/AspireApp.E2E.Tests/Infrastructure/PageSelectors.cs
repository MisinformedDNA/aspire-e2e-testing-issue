namespace AspireApp.E2E.Tests.Infrastructure;

public static class PageSelectors
{
    public const string NavBrand = "a.navbar-brand";

    public const string LoginTrigger =
        "a:has-text('Sign In'), button:has-text('Sign In'), a[href='/login']";

    public const string LoginForm = "#login-email, #login-password, #login-submit";

    public const string AuthenticatedUi =
        "button:has-text('Sign Out'), a:has-text('Sign Out'), form[action='/logout'] button";

    public const string ClerkModal = ""; // No longer used

    public const string NavAuthStateResolved =
        "button:has-text('Sign Out'), a:has-text('Sign Out'), a[href='/login'], a:has-text('Sign In')";

    public const string PublicPageContainer = ".public-page-container";
    public const string ProtectedPageContainer = ".protected-page-container";
}
