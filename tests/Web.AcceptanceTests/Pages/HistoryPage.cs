namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class HistoryPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/history";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Workout History");

    public Task AssertSessionVisible(string exerciseName) =>
        Assertions.Expect(Page.Locator("article", new() { HasText = exerciseName }).First).ToBeVisibleAsync();

    public Task AssertStatusTag(string exerciseName, string status) =>
        Assertions.Expect(Page.Locator("article", new() { HasText = exerciseName }).First.Locator(".status-tag"))
            .ToHaveTextAsync(status);

    public Task ClickViewFor(string exerciseName) =>
        Page.Locator("article", new() { HasText = exerciseName }).First.Locator("a:has-text('View')").ClickAsync();
}
