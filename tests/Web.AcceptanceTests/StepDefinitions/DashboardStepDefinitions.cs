namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class DashboardStepDefinitions(DashboardPage dashboardPage)
{
    [BeforeFeature("Dashboard")]
    public static async Task BeforeDashboardFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new DashboardPage(page));
    }

    [AfterFeature("Dashboard")]
    public static async Task AfterDashboardFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the dashboard page")]
    public Task GivenAnAuthenticatedUserVisitsTheDashboardPage() => dashboardPage.GotoAsync();

    [Then("the dashboard heading is visible")]
    public Task ThenTheDashboardHeadingIsVisible() => dashboardPage.AssertHeading();

    [Then("the personal records section is visible")]
    public Task ThenThePersonalRecordsSectionIsVisible() => dashboardPage.AssertPersonalRecordsSectionVisible();

    [Then("a link to the programme page is visible")]
    public Task ThenALinkToTheProgrammePageIsVisible() => dashboardPage.AssertProgrammeCardVisible();
}
