# Prensadão

Sistema de gestão de pedidos para uma hamburgueria, desenvolvido em ASP.NET Core com arquitetura em camadas. O projeto usa PostgreSQL, Entity Framework Core, RabbitMQ, Hangfire, NodaTime e Azure OpenAI para geração de sugestões de promoções.

## Documentação
A documentação técnica principal fica em `docs/`:

- `docs/README.md`: índice.
- `docs/ARCHITECTURE.md`: arquitetura, camadas e dependências.
- `docs/FEATURES.md`: funcionalidades existentes.
- `docs/AI_PROMOTIONS.md`: fluxo de promoções com IA.
- `docs/RUNBOOK.md`: comandos de execução, Docker, migrations e testes.
- `docs/rules/`: guardrails para manutenção por humanos, Codex e Claude.

## Tecnologias
- ASP.NET Core / .NET 8
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- Hangfire
- NodaTime
- Azure OpenAI
- xUnit, FakeItEasy, AutoFixture e FluentAssertions
- Docker / Docker Compose

## Comandos Principais
Execute a partir da raiz do repositório:

```powershell
dotnet restore Prensadao.sln
dotnet build Prensadao.sln
dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"
dotnet run --project Prensadao/Prensadao.API.csproj
docker compose up -d --build
```

Para detalhes de configuração, migrations, variáveis e troubleshooting, consulte `docs/RUNBOOK.md`.
