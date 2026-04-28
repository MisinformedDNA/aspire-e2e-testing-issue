namespace AspireApp.E2E.Tests;

public class AuthenticatedTests : BasePlaywrightTests
{
    [Test]
    public async Task Login_ShouldAuthenticateUser()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        var authenticated = await AuthenticateUserAsync(page);

        authenticated.Should().BeTrue("user should be authenticated after completing stub auth flow");

        var authUi = await page.QuerySelectorAsync(PageSelectors.AuthenticatedUi);
        authUi.Should().NotBeNull();
    }

    [Test]
    public async Task Login_ShouldNavigateToProtectedPage()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        var authenticated = await AuthenticateUserAsync(page);
        authenticated.Should().BeTrue();

        await page.GotoAsync($"{WebBaseUrl}/protected");
        await page.WaitForSelectorAsync(PageSelectors.ProtectedPageContainer, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 30_000,
        });

        var container = await page.QuerySelectorAsync(PageSelectors.ProtectedPageContainer);
        container.Should().NotBeNull();
    }

    [Test]
    public async Task Logout_ShouldShowLoginTrigger()
    {
        await using var context = await CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        var authenticated = await AuthenticateUserAsync(page);
        authenticated.Should().BeTrue();

        var signOutButton = await page.WaitForSelectorAsync(PageSelectors.AuthenticatedUi, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000,
        });
        await signOutButton!.ClickAsync();

        await page.WaitForSelectorAsync(PageSelectors.LoginTrigger, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15_000,
        });

        var loginTrigger = await page.QuerySelectorAsync(PageSelectors.LoginTrigger);
        loginTrigger.Should().NotBeNull();
    }
}
