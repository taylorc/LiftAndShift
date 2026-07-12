using LiftAndShift.Application.FunctionalTests;
using LiftAndShift.Domain.Constants;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class WeatherStepDefinitions(WeatherPage weatherPage)
{
    private const string userName = "administrator@localhost";
    private const string email = userName;
    private const string Password = "Administrator1!";
    private static ServiceProvider InfraProvider = null!;
    public static IServiceScope AppScope = null!;

    [BeforeFeature("Weather")]
    public static async Task BeforeWeatherFeature(IObjectContainer container)
    {
        
        //var connectionString = FunctionalTestSetup.ConnectionString;

        //// Strip any SSL/TLS requirements from the Aspire-generated Azure Postgres connection string
        //// — local RunAsContainer Postgres doesn't have SSL configured.
        //var csBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString) { SslMode = Npgsql.SslMode.Disable };
        //connectionString = csBuilder.ToString();

        //var infraServices = new ServiceCollection();

        //infraServices.AddLogging();
        //infraServices.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseNpgsql(connectionString));

        //infraServices
        //    .AddIdentityCore<ApplicationUser>()
        //    .AddRoles<IdentityRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>();

        //InfraProvider = infraServices.BuildServiceProvider();

        //AppScope = InfraProvider.CreateScope();

        //var userManager = AppScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        //var user = await userManager.FindByEmailAsync(email);

        //if(user == null)
        //{
        //    user = new ApplicationUser
        //    {
        //        UserName = userName,
        //        Email = email,
        //        EmailConfirmed = true
        //    };
        //    await userManager.CreateAsync(user, Password);
        //}

        //user.AlternatingLift = AlternatingLiftType.PendlayRow;
        //user.BodyWeight = 180;
        //user.PreferredUnit = WeightUnit.Kgs;
        //user.SquatStartingWeight = 120;
        //user.BenchPressStartingWeight = 80;
        //user.DeadliftStartingWeight = 150;
        //user.OverheadPressStartingWeight = 20;
        //user.AlternatingLiftStartingWeight = 60;
        //user.IsOnboarded = true;

        //await userManager.UpdateAsync(user);


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
        using var scope = InfraProvider.CreateScope();
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
