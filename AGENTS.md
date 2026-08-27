# Diretrizes do Repositório

Este arquivo é o ponto de entrada para agentes como Codex ao trabalhar no Prensadão. Antes de alterar código, leia este arquivo, o índice em `docs/README.md`, `docs/rules/00-principios.md` e as rules aplicáveis em `docs/rules/`.

## Visão Geral
`Prensadao.sln` é uma solução em camadas para uma API ASP.NET Core em .NET 8. O projeto usa PostgreSQL com Entity Framework Core, RabbitMQ para mensageria, Hangfire, NodaTime, Azure OpenAI para sugestões de promoções e testes automatizados com xUnit, FakeItEasy, AutoFixture e FluentAssertions.

## Estrutura
- `Prensadao/`: API ASP.NET Core, controllers, `Program.cs`, Swagger e pipeline HTTP.
- `Prensadao.Application/`: DTOs, contratos de serviço, serviços de negócio, helpers e constantes de mensageria.
- `Prensadao.Domain/`: entidades, enums, read models e interfaces de repositório.
- `Prensadao.Infra/`: EF Core, repositórios concretos, migrations, RabbitMQ, workers, Hangfire e Azure OpenAI.
- `Prensadao.Test/`: testes unitários por área.
- `docs/`: documentação técnica e rules para humanos e agentes.

## Regras Obrigatórias para Agentes
- Leia os arquivos próximos ao ponto de mudança antes de editar.
- Use SRP como princípio principal: cada classe, método ou módulo deve ter um motivo principal para mudar.
- Preserve a direção de dependências: API -> Application/Infra, Application -> Domain, Infra -> Application/Domain.
- Controllers devem ser finos; regra de negócio fica em Application ou Domain.
- Não acesse PostgreSQL, RabbitMQ, Hangfire ou Azure OpenAI diretamente em controllers.
- Não exponha entidades de domínio em endpoints quando houver DTO.
- Não registre segredos, connection strings reais, chaves RabbitMQ ou credenciais Azure OpenAI.
- Não edite migrations antigas, salvo pedido explícito e justificado.
- Preserve mudanças existentes do usuário; não reverta trabalho que você não fez.
- Evite refatorações amplas junto com correções pequenas.

## Build, Teste e Execução
Execute a partir da raiz:

- `dotnet restore Prensadao.sln`
- `dotnet build Prensadao.sln`
- `dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"`
- `dotnet run --project Prensadao/Prensadao.API.csproj`
- `docker compose up -d --build`
- `dotnet ef database update --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj`

Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`. Para alterações em regra de negócio, DTOs, contratos, repositórios, mensageria, workers ou IA, rode também testes relevantes ou `dotnet test Prensadao.Test/Prensadao.Test.csproj`.

## Git
Agentes só podem executar comandos `git` quando o usuário pedir explicitamente. Antes de criar commit, mostre a separação proposta dos arquivos por commit, sugira mensagens e aguarde confirmação.

Nunca execute comandos destrutivos ou de reescrita de histórico sem autorização explícita e específica, incluindo `git reset --hard`, `git checkout --`, `git clean`, `git rebase` e `git push --force`.

## Documentação de Referência
- `docs/rules/00-principios.md`: SRP, DRY, SOLID pragmático e comentários didáticos.
- `docs/ARCHITECTURE.md`: arquitetura e dependências.
- `docs/FEATURES.md`: funcionalidades existentes.
- `docs/AI_PROMOTIONS.md`: fluxo de promoções com IA.
- `docs/RUNBOOK.md`: execução local, Docker, migrations e testes.
- `docs/rules/`: regras específicas por área.
