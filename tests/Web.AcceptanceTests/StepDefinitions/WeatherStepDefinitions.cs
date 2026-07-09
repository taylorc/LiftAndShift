using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class WeatherStepDefinitions(WeatherPage weatherPage)
{
    private const string userName = "test@localhost";
    private const string email = userName;
    private const string Password = "Administrator1!";

    [BeforeFeature("Weather")]
    public static async Task BeforeWeatherFeature(IObjectContainer container)
    {
        using var scope = AspireSetup.InfraProvider.CreateScope();
        //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //await userManager.CreateAsync(new ApplicationUser
        //{
        //    Email = email,
        //    UserName = userName,
        //    AlternatingLift = AlternatingLiftType.PendlayRow,
        //    BodyWeight = 180,
        //    PreferredUnit = WeightUnit.Kgs,
        //    SquatStartingWeight = 120,
        //    BenchPressStartingWeight = 80,
        //    DeadliftStartingWeight = 150,
        //    OverheadPressStartingWeight = 20,
        //    AlternatingLiftStartingWeight = 60,
        //    IsOnboarded = true
        //}, Password);

        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await context.NewPageAsync();

        var loginPage = new LoginPage(page);
        await loginPage.GotoAsync();
        await loginPage.SetEmail(email);
        await loginPage.SetPassword(Password);
        await loginPage.ClickLogin();
        await Assertions.Expect(page.Locator("a:has-text('Log out')")).ToBeVisibleAsync();

        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new WeatherPage(page));
    }

    [AfterFeature("Weather")]
    public static async Task AfterWeatherFeature(IObjectContainer container)
    {
        using var scope = AspireSetup.InfraProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }

        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the weather page")]
    public Task GivenAnAuthenticatedUserVisitsTheWeatherPage() => weatherPage.GotoAsync();

    [Then("the weather forecast heading is {string}")]
    public Task ThenTheWeatherForecastHeadingIs(string text) => weatherPage.AssertHeading(text);

    [Then("the weather forecast table is displayed")]
    public Task ThenTheWeatherForecastTableIsDisplayed() => weatherPage.AssertTableVisible();

    [Then("{int} weather forecasts are shown")]
    public Task ThenWeatherForecastsAreShown(int count) => weatherPage.AssertRowCount(count);

}
