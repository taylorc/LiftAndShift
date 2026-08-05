namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class ProgrammeStepDefinitions(ProgrammePage programmePage)
{
    [BeforeFeature("Programme")]
    public static async Task BeforeProgrammeFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new ProgrammePage(page));
    }

    [AfterFeature("Programme")]
    public static async Task AfterProgrammeFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the programme page with no active programme")]
    public Task GivenAnAuthenticatedUserVisitsTheProgrammePageWithNoActiveProgramme() => programmePage.GotoAsync();

    [When("they start the {string} programme")]
    public Task TheyStartTheProgramme(string templateName) => programmePage.ClickStartTemplate(templateName);

    [Then("their active programme is {string}")]
    public Task TheirActiveProgrammeIs(string name) => programmePage.AssertActiveProgrammeName(name);

    [Then("their next session is visible")]
    public Task TheirNextSessionIsVisible() => programmePage.AssertNextSessionVisible();
}
