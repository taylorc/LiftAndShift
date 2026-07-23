namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class UserManagementStepDefinitions(UserManagementPage userManagementPage)
{
    [BeforeFeature("UserManagement")]
    public static async Task BeforeUserManagementFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new UserManagementPage(page));
    }

    [AfterFeature("UserManagement")]
    public static async Task AfterUserManagementFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user is on the account page")]
    public Task GivenAnAuthenticatedUserIsOnTheAccountPage() => userManagementPage.GotoAsync();

    [When("they submit updated starting weights")]
    public async Task TheySubmitUpdatedStartingWeights()
    {
        await userManagementPage.FillRequiredWeights("185", "125", "85", "52.5", "155", "62.5");
        await userManagementPage.ClickStartTraining();
    }

    [Then("saving their profile redirects them to the home page")]
    public Task SavingTheirProfileRedirectsThemToTheHomePage() => userManagementPage.AssertRedirectedToHome();
}
