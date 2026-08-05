namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class CalculatorsStepDefinitions(CalculatorsPage calculatorsPage)
{
    [BeforeFeature("Calculators")]
    public static async Task BeforeCalculatorsFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new CalculatorsPage(page));
    }

    [AfterFeature("Calculators")]
    public static async Task AfterCalculatorsFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the calculators page")]
    public Task GivenAnAuthenticatedUserVisitsTheCalculatorsPage() => calculatorsPage.GotoAsync();

    [When("they calculate plates for {int} kg")]
    public async Task TheyCalculatePlatesForKg(int weight)
    {
        await calculatorsPage.SetPlateTargetWeight(weight.ToString());
        await calculatorsPage.ClickCalculate();
    }

    [Then("the plate calculator result is displayed")]
    public Task ThenThePlateCalculatorResultIsDisplayed() => calculatorsPage.AssertPlateResultVisible();

    [When("they switch to the warmup calculator and calculate for {int} kg")]
    public async Task TheySwitchToTheWarmupCalculatorAndCalculateForKg(int weight)
    {
        await calculatorsPage.ClickWarmupCalculatorTab();
        await calculatorsPage.SetWarmupWorkingWeight(weight.ToString());
        await calculatorsPage.ClickCalculate();
    }

    [Then("the warmup calculator result is displayed")]
    public Task ThenTheWarmupCalculatorResultIsDisplayed() => calculatorsPage.AssertWarmupTableVisible();
}
