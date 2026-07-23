using System.Text.RegularExpressions;

namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class RegisterPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/register";

    public Task SetEmail(string email) => Page.FillAsync("#email", email);

    public Task SetPassword(string password) => Page.FillAsync("#password", password);

    public Task ClickRegister() => Page.Locator("button[type='submit']").ClickAsync();

    public Task AssertRedirectedToLogin() => Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/login$"));

    public Task AssertStillOnRegisterPage() => Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/register$"));

    public Task AssertPasswordHelperTextVisible() =>
        Assertions.Expect(Page.Locator("#password-helper")).ToHaveTextAsync("Password must be at least 6 characters.");
}
