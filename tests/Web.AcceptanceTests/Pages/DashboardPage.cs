namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class DashboardPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/dashboard";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Dashboard");

    public Task AssertPersonalRecordsSectionVisible() =>
        Assertions.Expect(Page.Locator("h2:has-text('Personal Records')")).ToBeVisibleAsync();

    public Task AssertProgrammeCardVisible() =>
        Assertions.Expect(Page.Locator("a[href='/programme']").First).ToBeVisibleAsync();

    public Task ClickProgrammeLink() => Page.Locator("a[href='/programme']").First.ClickAsync();

    public Task AssertPersonalRecordVisible(string exerciseName) =>
        Assertions.Expect(Page.Locator("table tbody tr", new() { HasText = exerciseName })).ToBeVisibleAsync();
}
