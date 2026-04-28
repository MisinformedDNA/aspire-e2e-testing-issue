namespace AspireApp.E2E.Tests;

public class UnauthenticatedTests : BasePlaywrightTests
{
    [Test]
    public async Task HomePage_ShouldLoadSuccessfully()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(WebBaseUrl);
        await page.WaitForSelectorAsync(PageSelectors.NavBrand, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });

        var brand = await page.TextContentAsync(PageSelectors.NavBrand);
        brand.Should().Contain("AspireApp");
    }

    [Test]
    public async Task PublicPage_ShouldBePubliclyAccessible()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{WebBaseUrl}/public");
        await page.WaitForSelectorAsync(PageSelectors.PublicPageContainer, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });

        var h1 = await page.TextContentAsync($"{PageSelectors.PublicPageContainer} h1");
        h1.Should().Be("Public Page");

        var link = await page.QuerySelectorAsync($"{PageSelectors.PublicPageContainer} a.btn-action");
        link.Should().NotBeNull();
        var href = await link!.GetAttributeAsync("href");
        href.Should().Contain("/protected");
    }

    [Test]
    public async Task Navigation_ShouldShowLogin_WhenNotAuthenticated()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(WebBaseUrl);
        await page.WaitForSelectorAsync(PageSelectors.LoginTrigger, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });

        var loginTrigger = await page.QuerySelectorAsync(PageSelectors.LoginTrigger);
        loginTrigger.Should().NotBeNull();
    }

    [Test]
    public async Task ProtectedPage_WhenUnauthenticated_ShouldShowClerkSignIn()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{WebBaseUrl}/protected");

        // Wait for either the Clerk modal or a redirect to sign-in
        await page.WaitForSelectorAsync(
            $"{PageSelectors.ClerkModal}, {PageSelectors.LoginTrigger}",
            new PageWaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000,
            });

        var modal = await page.QuerySelectorAsync(PageSelectors.ClerkModal);
        var loginTrigger = await page.QuerySelectorAsync(PageSelectors.LoginTrigger);
        (modal is not null || loginTrigger is not null).Should().BeTrue();
    }
}
