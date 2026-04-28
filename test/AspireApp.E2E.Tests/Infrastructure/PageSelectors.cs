namespace AspireApp.E2E.Tests.Infrastructure;

public static class PageSelectors
{
    public const string NavBrand = "a.navbar-brand";

    public const string LoginTrigger =
        "button:has-text('Sign in'), a:has-text('Sign in'), a[role='button']:has-text('Sign in'), " +
        "button:has-text('Sign In'), a:has-text('Sign In')";

    public const string AuthenticatedUi =
        "button:has-text('Sign Out'), a:has-text('Sign Out'), .authenticated-nav-item";

    public const string ClerkModal =
        ".cl-rootBox, .cl-signIn-root, .cl-modalContent, .cl-signIn-start, [data-clerk-component]";

    public const string NavAuthStateResolved =
        "button:has-text('Sign Out'), a:has-text('Sign Out'), " +
        "button:has-text('Sign in'), a:has-text('Sign in'), button:has-text('Sign In'), a:has-text('Sign In')";

    public const string PublicPageContainer = ".public-page-container";
    public const string ProtectedPageContainer = ".protected-page-container";
}
