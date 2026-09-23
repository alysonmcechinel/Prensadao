# Diretrizes para Agentes

Este é o ponto de entrada único para agentes de desenvolvimento que trabalham no Prensadão. Ferramentas compatíveis com `AGENTS.md` devem seguir estas instruções; não mantenha arquivos equivalentes específicos por ferramenta.

Antes de alterar qualquer arquivo:

1. Leia `docs/README.md` para localizar a documentação da área.
2. Leia `docs/rules/00-principios.md` e as regras aplicáveis listadas em `docs/rules/README.md`.
3. Antes de implementar ou refatorar qualquer regra de negócio, consulte obrigatoriamente o arquivo `docs/GLOSSARY.md` para garantir o uso correto dos termos de domínio.
4. Leia os arquivos próximos ao ponto de mudança.
5. Confirme que a documentação descreve o comportamento atual antes de usá-la como base.

## Visão Geral

`Prensadao.sln` é uma solução em camadas para uma API ASP.NET Core em .NET 8. O projeto usa PostgreSQL com Entity Framework Core, RabbitMQ para mensageria, Hangfire, NodaTime, Azure OpenAI para sugestões de promoções e testes automatizados com xUnit, FakeItEasy, AutoFixture e FluentAssertions.

## Estrutura

- `Prensadao/`: API ASP.NET Core, controllers, `Program.cs`, Swagger e pipeline HTTP.
- `Prensadao.Application/`: DTOs, contratos de serviço, serviços de negócio, helpers e constantes de mensageria.
- `Prensadao.Domain/`: entidades, enums, read models e interfaces de repositório.
- `Prensadao.Infra/`: EF Core, repositórios concretos, migrations, RabbitMQ, workers, Hangfire e Azure OpenAI.
- `Prensadao.Test/`: testes unitários por área.
- `docs/`: documentação técnica, operacional e regras de manutenção.

## Regras Obrigatórias

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

## Diretrizes de Discussão e Design

- Ao avaliar arquiteturas, RFCs ou propostas de alteração, comece obrigatoriamente criticando a abordagem e apontando potenciais buracos antes de sugerir qualquer solução.
- Apresente ao menos duas alternativas de implementação, destacando os trade-offs e o impacto nos padrões existentes.

## Build, Teste e Execução

Os comandos oficiais ficam em `docs/RUNBOOK.md`. Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`. Para alterações em regra de negócio, DTOs, contratos, repositórios, mensageria, workers ou IA, rode também testes relevantes ou `dotnet test Prensadao.Test/Prensadao.Test.csproj`.

Mudanças exclusivamente documentais não exigem build, mas devem ter links, caminhos, nomes de tipos e comandos conferidos.

## Git

Agentes só podem executar comandos `git` quando o usuário pedir explicitamente. Antes de criar commit, mostre a separação proposta dos arquivos por commit, sugira mensagens e aguarde confirmação.

Nunca execute comandos destrutivos ou de reescrita de histórico sem autorização explícita e específica, incluindo `git reset --hard`, `git checkout --`, `git clean`, `git rebase` e `git push --force`.

## Documentação de Referência

- `docs/README.md`: índice, rotas de leitura e responsabilidade de cada documento.
- `docs/ARCHITECTURE.md`: arquitetura e dependências.
- `docs/FEATURES.md`: funcionalidades existentes.
- `docs/GLOSSARY.md`: significado dos termos de negócio e ambiguidades conhecidas.
- `docs/AI_PROMOTIONS.md`: fluxo de promoções com IA.
- `docs/RUNBOOK.md`: execução local, Docker, migrations e testes.
- `docs/ROADMAP.md`: pendências e decisões ainda não implementadas.
- `docs/rules/README.md`: índice de regras por tipo de mudança.
