namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class ProgrammePage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/programme";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Programme");

    public Task ClickStartTemplate(string templateName) =>
        Page.Locator($"button:has-text('Start {templateName}')").ClickAsync();

    public Task AssertActiveProgrammeName(string name) =>
        Assertions.Expect(Page.Locator("article header strong", new() { HasText = name }).First).ToBeVisibleAsync();

    public Task AssertNextSessionVisible() =>
        Assertions.Expect(Page.Locator("h2", new() { HasText = "Next Session" })).ToBeVisibleAsync();

    public Task ClickStartThisSession() => Page.Locator("button:has-text('Start This Session')").ClickAsync();
}
