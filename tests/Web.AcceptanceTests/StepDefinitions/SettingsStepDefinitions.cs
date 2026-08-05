namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class SettingsStepDefinitions(SettingsPage settingsPage)
{
    [BeforeFeature("Settings")]
    public static async Task BeforeSettingsFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new SettingsPage(page));
    }

    [AfterFeature("Settings")]
    public static async Task AfterSettingsFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the settings page")]
    public Task GivenAnAuthenticatedUserVisitsTheSettingsPage() => settingsPage.GotoAsync();

    [When("they select pounds as their weight unit")]
    public Task TheySelectPoundsAsTheirWeightUnit() => settingsPage.SelectLbs();

    [When("they save their settings")]
    public Task TheySaveTheirSettings() => settingsPage.ClickSaveSettings();

    [Then("a settings saved confirmation is displayed")]
    public Task ASettingsSavedConfirmationIsDisplayed() => settingsPage.AssertSavedMessageVisible();

    [Then("their preferred unit remains pounds after reloading the page")]
    public async Task TheirPreferredUnitRemainsPoundsAfterReloadingThePage()
    {
        await settingsPage.GotoAsync();
        var isLbsChecked = await settingsPage.IsLbsChecked();
        isLbsChecked.ShouldBeTrue();
    }
}
