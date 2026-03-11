# Repository Guidelines

## Project Structure & Module Organization
`Prensadao.sln` ties together a layered .NET 8 solution. `Prensadao/` contains the ASP.NET Core API entrypoint, controllers, `Program.cs`, and environment settings. `Prensadao.Application/` holds DTOs, service contracts, and business services. `Prensadao.Domain/` contains entities, enums, read models, and repository interfaces. `Prensadao.Infra/` implements persistence, EF Core migrations/configurations, RabbitMQ, workers, and external integrations such as Azure OpenAI. `Prensadao.Test/` contains unit tests grouped by application area.

## Build, Test, and Development Commands
Run from the repository root:

- `dotnet restore Prensadao.sln` restores NuGet packages.
- `dotnet build Prensadao.sln` compiles all projects and catches analyzer/compiler issues.
- `dotnet run --project Prensadao/Prensadao.API.csproj` starts the API locally.
- `dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"` runs xUnit tests with Coverlet coverage output.
- `docker compose up -d --build` starts PostgreSQL, RabbitMQ, and the Hangfire worker for local integration flows.
- `dotnet ef database update --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj` applies EF Core migrations.

## Coding Style & Naming Conventions
Follow existing C# conventions: 4-space indentation, `PascalCase` for public types/members, `camelCase` for locals/parameters, and `_camelCase` for private readonly fields. Keep one class per file and match the file name to the main type, for example `ProductService.cs` or `OrderController.cs`. Prefer constructor injection and keep controller logic thin by delegating to application services. Use descriptive DTO suffixes such as `RequestDto` and `ResponseDto`.

## Testing Guidelines
Tests use xUnit with FakeItEasy, AutoFixture, and FluentAssertions. Place new tests in `Prensadao.Test/<Layer>/` and name files after the subject under test, for example `CustomerServiceTest.cs`. Use `Method_ShouldExpectedBehavior_WhenCondition` naming for test methods. Add or update tests whenever service rules, repository contracts, or message-processing behavior changes.

## Commit & Pull Request Guidelines
Recent history favors short, imperative commit subjects with prefixes such as `refactor:`, `add:`, or plain action phrases like `Switch Hangfire to PostgreSQL`. Keep commits focused on one concern. Pull requests should include a concise summary, impacted projects, test evidence (`dotnet test` output or coverage note), linked issue/task, and screenshots or sample payloads when API behavior changes.

## Configuration & Security Tips
Keep secrets out of source control. Use environment variables or local user secrets for connection strings, RabbitMQ credentials, and Azure OpenAI settings. Treat `appsettings.Development.json` as local-only defaults and update EF migrations only when the model changes in `Prensadao.Infra/Persistence`.
