namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class CalculatorsPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/calculators";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Calculators");

    public Task ClickWarmupCalculatorTab() => Page.Locator("button:has-text('Warmup Calculator')").ClickAsync();

    public Task SetPlateTargetWeight(string value) => Page.Locator("section form input[type='number']").First.FillAsync(value);

    public Task SetWarmupWorkingWeight(string value) => Page.Locator("section form input[type='number']").First.FillAsync(value);

    public Task ClickCalculate() => Page.Locator("button[type='submit']:has-text('Calculate')").ClickAsync();

    public Task AssertPlateResultVisible() => Assertions.Expect(Page.Locator("text=Actual weight:")).ToBeVisibleAsync();

    public Task AssertWarmupTableVisible() => Assertions.Expect(Page.Locator("table")).ToBeVisibleAsync();
}
