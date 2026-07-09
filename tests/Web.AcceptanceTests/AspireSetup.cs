using Aspire.Hosting;
using System.Diagnostics;

namespace LiftAndShift.Web.AcceptanceTests;

[SetUpFixture]
public class AspireSetup
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    public static IDistributedApplicationTestingBuilder Builder { get; private set; } = null!;
    public static DistributedApplication App { get; private set; } = null!;
    public static ServiceProvider InfraProvider { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var cts = new CancellationTokenSource(DefaultTimeout);
        var cancellationToken = cts.Token;

        Builder = await DistributedApplicationTestingBuilder
             .CreateAsync<Projects.AppHost>(
                args: [],
                configureBuilder: (options, _) =>
                {
                    options.DisableDashboard = false; // Enable the dashboard for testing purposes
                });



        Builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

        Builder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(Builder.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });

        Builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        App = await Builder
            .BuildAsync(cancellationToken)
            .WaitAsync(cancellationToken);

        await App
            .StartAsync(cancellationToken)
            .WaitAsync(cancellationToken);

        //var connectionString = await App.GetConnectionStringAsync(Services.Database, cancellationToken);

        // Strip any SSL/TLS requirements from the Aspire-generated Azure Postgres connection string
        // — local RunAsContainer Postgres doesn't have SSL configured.
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

        await Task.WhenAll(
            App.ResourceNotifications.WaitForResourceHealthyAsync(Services.WebApi, cancellationToken).WaitAsync(cancellationToken),
            App.ResourceNotifications.WaitForResourceHealthyAsync(Services.WebFrontend, cancellationToken).WaitAsync(cancellationToken));
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await InfraProvider.DisposeAsync();
        await App.DisposeAsync();
    }
}
