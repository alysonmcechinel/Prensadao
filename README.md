# Prensadão

Sistema de gestão de pedidos para uma hamburgueria, desenvolvido em ASP.NET Core com arquitetura em camadas. O projeto usa PostgreSQL, Entity Framework Core, RabbitMQ, Hangfire, NodaTime e Azure OpenAI para geração de sugestões de promoções.

## Documentação

A documentação técnica principal fica em `docs/`:

- `AGENTS.md`: instruções obrigatórias para agentes de desenvolvimento.
- `docs/README.md`: índice e rotas de leitura.
- `docs/ARCHITECTURE.md`: arquitetura, camadas e dependências.
- `docs/FEATURES.md`: funcionalidades existentes.
- `docs/GLOSSARY.md`: vocabulário canônico dos termos de negócio e técnicos próprios do Prensadão.
- `docs/AI_PROMOTIONS.md`: fluxo de promoções com IA.
- `docs/RUNBOOK.md`: comandos de execução, Docker, migrations e testes.
- `docs/ROADMAP.md`: pendências e decisões futuras.
- `docs/rules/`: regras de manutenção por área.

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
