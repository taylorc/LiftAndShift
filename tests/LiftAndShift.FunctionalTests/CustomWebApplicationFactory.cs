using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiftAndShift.FunctionalTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
  // appsettings.Testing.json's own SqliteConnection value is not a valid connection string
  // (missing "Data Source="), and a ":memory:" database is also per-connection, so seeded
  // data from CreateHost wouldn't be visible to the app's own request-time connections
  // anyway. An environment variable reliably overrides appsettings.*.json regardless of
  // configuration source ordering, so it points every connection at one per-run temp file.
  static CustomWebApplicationFactory()
  {
    string connectionString =
      $"Data Source={Path.Combine(Path.GetTempPath(), $"liftandshift-functionaltests-{Guid.NewGuid():N}.sqlite")}";
    Environment.SetEnvironmentVariable("ConnectionStrings__SqliteConnection", connectionString);
  }

  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Testing"); // will not send real emails
    var host = builder.Build();
    host.Start();

    // Get service provider.
    var serviceProvider = host.Services;

    // Create a scope to obtain a reference to the database
    // context (AppDbContext).
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<AppDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      try
      {
        db.Database.EnsureCreated();

        // Seed the database with test data only if it has not been seeded yet.
        SeedData.InitializeAsync(db).Wait();
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }
}
