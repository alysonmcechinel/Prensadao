# Diretrizes do Repositório

## Estrutura do Projeto e Organização dos Módulos
`Prensadao.sln` conecta uma solução em camadas com .NET 8. `Prensadao/` contém o ponto de entrada da API ASP.NET Core, controllers, `Program.cs` e configurações de ambiente. `Prensadao.Application/` contém DTOs, contratos de serviço e serviços de negócio. `Prensadao.Domain/` contém entidades, enums, modelos de leitura e interfaces de repositório. `Prensadao.Infra/` implementa persistência, migrações/configurações do EF Core, RabbitMQ, workers e integrações externas, como Azure OpenAI. `Prensadao.Test/` contém testes unitários agrupados por área da aplicação.

## Comandos de Build, Teste e Desenvolvimento
Execute a partir da raiz do repositório:

- `dotnet restore Prensadao.sln` restaura os pacotes NuGet.
- `dotnet build Prensadao.sln` compila todos os projetos e identifica problemas de compilador e analisadores.
- `dotnet run --project Prensadao/Prensadao.API.csproj` inicia a API localmente.
- `dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"` executa os testes xUnit com saída de cobertura do Coverlet.
- `docker compose up -d --build` inicia PostgreSQL, RabbitMQ e o worker do Hangfire para fluxos locais de integração.
- `dotnet ef database update --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj` aplica as migrações do EF Core.

## Estilo de Código e Convenções de Nomenclatura
Siga as convenções C# já existentes: indentação de 4 espaços, `PascalCase` para tipos/membros públicos, `camelCase` para variáveis locais/parâmetros e `_camelCase` para campos privados somente leitura. Mantenha uma classe por arquivo e faça o nome do arquivo corresponder ao tipo principal, por exemplo `ProductService.cs` ou `OrderController.cs`. Prefira injeção via construtor e mantenha a lógica dos controllers enxuta, delegando para os serviços da aplicação. Use sufixos descritivos para DTOs, como `RequestDto` e `ResponseDto`.

## Diretrizes de Testes
Os testes usam xUnit com FakeItEasy, AutoFixture e FluentAssertions. Coloque novos testes em `Prensadao.Test/<Layer>/` e nomeie os arquivos de acordo com o item testado, por exemplo `CustomerServiceTest.cs`. Use o padrão `Method_ShouldExpectedBehavior_WhenCondition` para nomear os métodos de teste. Adicione ou atualize testes sempre que regras de serviço, contratos de repositório ou o comportamento de processamento de mensagens mudarem.

## Diretrizes de Commit e Pull Request
O histórico recente favorece mensagens de commit curtas, no imperativo, com prefixos como `refactor:`, `add:` ou frases diretas como `Switch Hangfire to PostgreSQL`. Mantenha os commits focados em uma única preocupação.
O assistente não deve executar nenhum comando `git` em hipótese alguma, incluindo consultas, staging, commits, merges, rebases, checkouts, resets, pulls, pushes, fetches, tags, diffs, status ou logs. Todo o controle de versão deve permanecer exclusivamente nas mãos do usuário. Quando necessário, o assistente pode apenas orientar em texto sobre quais comandos o usuário pode executar manualmente. Pull requests devem incluir um resumo conciso, projetos impactados, evidências de teste (saída do `dotnet test` ou nota de cobertura), issue/tarefa vinculada e screenshots ou payloads de exemplo quando o comportamento da API mudar.

## Dicas de Configuração e Segurança
Mantenha segredos fora do controle de versão. Use variáveis de ambiente ou user secrets locais para strings de conexão, credenciais do RabbitMQ e configurações do Azure OpenAI. Trate `appsettings.Development.json` como padrão apenas local e atualize as migrações do EF apenas quando o modelo mudar em `Prensadao.Infra/Persistence`.

## Documentação e Comentários de Código
