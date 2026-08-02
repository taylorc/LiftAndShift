namespace LiftAndShift.Web.AcceptanceTests;

public static class AuthSteps
{
    public const string AdministratorEmail = "administrator@localhost.com";
    public const string AdministratorPassword = "Administrator1!";

    public static async Task<IPage> LoginAsAdministrator(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        var loginPage = new LoginPage(page);

        await loginPage.GotoAsync();
        await loginPage.SetEmail(AdministratorEmail);
        await loginPage.SetPassword(AdministratorPassword);
        await loginPage.ClickLogin();

        await Assertions.Expect(page.Locator("a:has-text('Log out')")).ToBeVisibleAsync();

        return page;
    }

}
