namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class RegisterStepDefinitions(RegisterPage registerPage)
{
    [BeforeFeature("Register")]
    public static async Task BeforeRegisterFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new RegisterPage(page));
    }

    [AfterFeature("Register")]
    public static async Task AfterRegisterFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("a visitor is on the register page")]
    public Task GivenAVisitorIsOnTheRegisterPage() => registerPage.GotoAsync();

    [When("they register with a valid email and password")]
    public async Task TheyRegisterWithAValidEmailAndPassword()
    {
        var email = $"testuser-{Guid.NewGuid():N}@example.com";
        await registerPage.SetEmail(email);
        await registerPage.SetPassword("ValidPass1!");
        await registerPage.ClickRegister();
    }

    [Then("they are redirected to the login page")]
    public Task TheyAreRedirectedToTheLoginPage() => registerPage.AssertRedirectedToLogin();

    [When("they submit the registration form with a password that is too short")]
    public async Task TheySubmitTheRegistrationFormWithAPasswordThatIsTooShort()
    {
        var email = $"testuser-{Guid.NewGuid():N}@example.com";
        await registerPage.SetEmail(email);
        await registerPage.SetPassword("abc");
        await registerPage.ClickRegister();
    }

    [Then("a password validation message is displayed")]
    public async Task APasswordValidationMessageIsDisplayed()
    {
        await registerPage.AssertPasswordHelperTextVisible();
        await registerPage.AssertStillOnRegisterPage();
    }
}
