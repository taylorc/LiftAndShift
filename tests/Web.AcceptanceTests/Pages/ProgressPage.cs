namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class ProgressPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/progress";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Progress");

    public async Task SelectExerciseByName(string name)
    {
        var select = Page.Locator("select").First;
        var value = await select.Locator("option", new() { HasText = name }).First.GetAttributeAsync("value");
        await select.SelectOptionAsync(new SelectOptionValue { Value = value });
    }

    public Task AssertTableRowVisible() => Assertions.Expect(Page.Locator("table tbody tr").First).ToBeVisibleAsync();

    public Task AssertNoDataMessageVisible() =>
        Assertions.Expect(Page.Locator("text=No completed sessions with this exercise yet.")).ToBeVisibleAsync();
}
