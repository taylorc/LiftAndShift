# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

LiftAndShift is a Starting Strength-style lifting tracker (see `StrartingStrength-UserStories.md` for the product
spec: onboarding, programme generation, workout logging, plate/warmup calculators, progress tracking). It was
scaffolded from the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture)
(v10.8.0) and layers a Nuxt frontend and .NET Aspire orchestration on top.

## Commands

Run everything from the repo root unless noted.

```bash
dotnet build                                     # build the whole solution
dotnet run --project .\src\AppHost                # run via Aspire (starts API, Postgres, Vite/Nuxt frontend; opens Aspire dashboard)
dotnet test                                      # run all tests (unit, integration, functional, acceptance)
dotnet test --filter "FullyQualifiedName~LogWorkoutCommandTests"   # run a single test class
dotnet test tests/Application.UnitTests           # run a single test project
```

Frontend (`src/Web/ClientApp`, Nuxt 3 + Pinia + Vite):
```bash
npm run start   # generate typed API client (nswag) then `nuxt dev` — normally launched for you via AppHost, not run standalone
npm run build   # generate-api then nuxt build
```
The generated API client is produced by `nswag` (`generate-api` script) from the running Web API's OpenAPI document —
regenerate it after changing any endpoint signature/DTO.

Scaffolding new Application features (from `src/Application/`):
```bash
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```
If `ca-usecase` isn't found: `dotnet new install Clean.Architecture.Solution.Template::10.8.0`.

## Architecture

Clean Architecture, four `src` layers plus a Web API and Aspire host:

- **Domain** — entities (`src/Domain/Entities`), value objects, enums, domain events. No dependencies on other layers.
- **Application** — CQRS via MediatR, organized as **vertical slices per feature** (e.g. `Workouts/Commands/LogWorkout/LogWorkoutCommand.cs`
  contains the command record, handler, and FluentValidation validator together in one file). Cross-cutting concerns
  run as MediatR pipeline behaviours registered in `Application/DependencyInjection.cs`, in order: logging
  (pre-processor) → `UnhandledExceptionBehaviour` → `AuthorizationBehaviour` (enforces `[Authorize]` on
  commands/queries) → `ValidationBehaviour` (runs FluentValidation validators, throws on failure) → `PerformanceBehaviour`.
  Entity/DTO mapping uses Mapster, configured centrally in the same file.
- **Infrastructure** — EF Core (`ApplicationDbContext`, Npgsql/Postgres), ASP.NET Core Identity, SaveChanges interceptors
  for auditing and domain event dispatch (`Infrastructure/Data/Interceptors`).
- **Web** — Minimal API host. Endpoints are grouped via `IEndpointGroup` (`src/Web/Endpoints/*.cs`): each group is a
  static class implementing `Map(RouteGroupBuilder)`, auto-discovered and registered by `WebApplicationExtensions.MapEndpoints`
  under `/api/{ClassName}` (override `IEndpointGroup.RoutePrefix` for nested routes). Handlers are named static methods
  (never lambdas — `EndpointRouteBuilderExtensions` guards against anonymous delegates since the method name becomes
  the OpenAPI `operationId` used by nswag codegen) that resolve `ISender` and dispatch a MediatR command/query. Custom
  exceptions (`ValidationException`, `NotFoundException`, `UnauthorizedAccessException`, `ForbiddenAccessException`)
  are translated to RFC 9110 `ProblemDetails` by `ProblemDetailsExceptionHandler`.
- **Shared** — contracts shared across host boundaries (e.g. Aspire service names in `MediatorContracts`/`Services`).
- **AppHost / ServiceDefaults** — .NET Aspire orchestration. `AppHost` wires up an Azure Postgres Flexible Server
  (containerized locally with pgAdmin), the Web API project, and — only in run mode — the Vite-hosted Nuxt frontend
  (`ClientApp`) as a child process wired to the API via service discovery.
- **Web/ClientApp** — Nuxt 3 SPA/SSR frontend, Pinia for state, Pico CSS. Talks to the API through the nswag-generated
  typed client; in dev, Nitro route rules proxy `/api/**`, `/openapi/**`, `/scalar/**` to the API service discovered
  via Aspire env vars (`services__webapi__https__0` / `http__0`).

Adding a new feature typically touches: a Domain entity/value object (if new state) → an Application command/query
vertical slice (with validator) → EF configuration in `Infrastructure/Data/Configurations` + migration → an endpoint
method in the matching `Web/Endpoints/*.cs` group → regenerate the nswag client for the frontend.

Tests mirror this layering: `Domain.UnitTests`, `Application.UnitTests` (handler/validator logic, mocked dependencies),
`Application.FunctionalTests` (full MediatR pipeline against a real/test database), and `Infrastructure.IntegrationTests`.
`TestAppHost` provides the Aspire-based test harness used to spin up dependencies for functional/integration tests.

Browser-driven acceptance tests live with the frontend in `src/Web/ClientApp/e2e` (Playwright + TypeScript, plain
`test()` blocks and page objects — no Gherkin), run with `npm run test:e2e`. Playwright's `globalSetup` spawns
`tests/AcceptanceTestHost`, a console app that starts the whole Aspire stack via `Aspire.Hosting.Testing`, prints
`E2E_BASE_URL=<frontend url>`, and shuts down when its stdin closes.

## Agent skills

### Issue tracker

Issues live in GitHub Issues on `taylorc/LiftAndShift`, using the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Triage labels

Default canonical labels (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout: `CONTEXT.md` + `docs/adr/` at the repo root. See `docs/agents/domain.md`.
