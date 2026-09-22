using System.Net.Sockets;

#pragma warning disable ASPIREJAVASCRIPT001 // Aspire.Hosting.JavaScript is evaluation-only for now

var builder = DistributedApplication.CreateBuilder(args);

// Papercut SMTP container for email testing
var papercut = builder.AddContainer("papercut", "jijiechen/papercut", "latest")
  .WithEndpoint("smtp", e =>
  {
    e.TargetPort = 25;   // container port
    e.Port = 25;         // host port
    e.Protocol = ProtocolType.Tcp;
    e.UriScheme = "smtp";
  })
  .WithEndpoint("ui", e =>
  {
    e.TargetPort = 37408;
    e.Port = 37408;
    e.UriScheme = "http";
  });

// Add the web project with the database connection
var web = builder.AddProject<Projects.LiftAndShift_Web>("web")
  .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
  .WithEnvironment("Papercut__Smtp__Url", papercut.GetEndpoint("smtp"))
  .WaitFor(papercut);

// Nuxt frontend - `npm run dev`, pointed at the API's plain-HTTP endpoint (see MiddlewareConfig.cs
// for why Development skips HTTPS redirection: Nuxt's SSR fetch runs in Node, which doesn't trust
// the ASP.NET Core dev cert)
builder.AddJavaScriptApp("clientapp", "../LiftAndShift.Web/ClientApp", "dev")
  .WithEnvironment("NUXT_PUBLIC_API_BASE", web.GetEndpoint("http"))
  .WithHttpEndpoint(port: 3000, env: "PORT")
  .WithExternalHttpEndpoints()
  .WaitFor(web);

builder
  .Build()
  .Run();
