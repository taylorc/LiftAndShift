using Aspire.Hosting;
using Aspire.Hosting.Testing;
using LiftAndShift.Shared;

// Starts the full Aspire stack (Postgres, Web API, Nuxt frontend) for the ClientApp
// Playwright suite, prints the resolved frontend URL, then shuts down when stdin closes.

// The Nuxt dev server proxies /api/** to the API's HTTPS endpoint, whose ASP.NET dev
// certificate Node does not trust. The Vite child process inherits this from us.
Environment.SetEnvironmentVariable("NODE_TLS_REJECT_UNAUTHORIZED", "0");

var startupTimeout = TimeSpan.FromSeconds(180);
using var cts = new CancellationTokenSource(startupTimeout);
var cancellationToken = cts.Token;

var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(
    args: [],
    configureBuilder: (options, _) => options.DisableDashboard = true);

builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

await using var app = await builder.BuildAsync(cancellationToken).WaitAsync(cancellationToken);

await app.StartAsync(cancellationToken).WaitAsync(cancellationToken);

await Task.WhenAll(
    app.ResourceNotifications.WaitForResourceHealthyAsync(Services.WebApi, cancellationToken).WaitAsync(cancellationToken),
    app.ResourceNotifications.WaitForResourceHealthyAsync(Services.WebFrontend, cancellationToken).WaitAsync(cancellationToken));

var frontendUrl = app.GetEndpoint(Services.WebFrontend).ToString().TrimEnd('/');

Console.WriteLine($"E2E_BASE_URL={frontendUrl}");
Console.Out.Flush();

// Block until the parent process closes stdin, which is our shutdown signal.
await Console.In.ReadToEndAsync();

await app.StopAsync();
