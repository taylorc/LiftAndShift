namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class SettingsPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/settings";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Settings");

    public Task SelectLbs() => Page.Locator("input[type='radio'][value='lbs']").CheckAsync();

    public Task SelectKg() => Page.Locator("input[type='radio'][value='kg']").CheckAsync();

    public Task<bool> IsLbsChecked() => Page.Locator("input[type='radio'][value='lbs']").IsCheckedAsync();

    public Task ClickSaveSettings() => Page.Locator("button:has-text('Save Settings')").ClickAsync();

    public Task AssertSavedMessageVisible() => Assertions.Expect(Page.Locator("text=Settings saved.")).ToBeVisibleAsync();
}
