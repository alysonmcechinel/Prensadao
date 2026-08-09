# Diretrizes do Repositório

## Visão Geral
`Prensadao.sln` é uma solução em camadas para uma API ASP.NET Core em .NET 8. O projeto usa PostgreSQL com Entity Framework Core, RabbitMQ para mensageria, Hangfire, NodaTime, Azure OpenAI para sugestões de promoções e testes automatizados com xUnit, FakeItEasy, AutoFixture e FluentAssertions.

Antes de alterar código, leia os arquivos próximos ao ponto de mudança e preserve o estilo já existente. Mantenha mudanças pequenas, focadas e coerentes com a separação entre API, Application, Domain e Infra.

## Estrutura do Projeto
- `Prensadao/`: API ASP.NET Core, controllers, `Program.cs`, `appsettings*.json`, Swagger e configuração do pipeline HTTP.
- `Prensadao.Application/`: DTOs, contratos de serviço, serviços de negócio, helpers, constantes de mensageria e exceções de aplicação.
- `Prensadao.Domain/`: entidades, enums, read models e interfaces de repositório. Não deve depender de Application, Infra ou API.
- `Prensadao.Infra/`: EF Core, configurações de entidades, repositórios concretos, migrations, RabbitMQ, workers, Hangfire, middleware e integrações externas.
- `Prensadao.Test/`: testes unitários organizados por área, com foco em serviços de aplicação e componentes de infraestrutura.

## Comandos de Build, Teste e Desenvolvimento
Execute a partir da raiz do repositório:

- `dotnet restore Prensadao.sln` restaura os pacotes NuGet.
- `dotnet build Prensadao.sln` compila todos os projetos.
- `dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"` executa os testes com cobertura.
- `dotnet run --project Prensadao/Prensadao.API.csproj` inicia a API localmente.
- `docker compose up -d --build` inicia os serviços locais necessários para fluxos de integração.
- `dotnet ef database update --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj` aplica migrations.
- `dotnet ef migrations add NomeDaMigration --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj --output-dir Persistence/Migrations` cria migrations quando o modelo persistido mudar.

Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`. Para alterações em regras de negócio, repositórios, mensageria, DTOs ou contratos públicos, rode também os testes relevantes ou `dotnet test Prensadao.Test/Prensadao.Test.csproj`.

## Arquitetura e Dependências
- Controllers devem ser finos: validar entrada básica quando necessário, chamar serviços de aplicação e retornar HTTP responses. Não coloque regra de negócio, acesso a banco, RabbitMQ ou Azure OpenAI diretamente em controllers.
- Serviços em `Prensadao.Application/Services` concentram regras de negócio, validações de fluxo, orquestração de repositórios e publicação de mensagens.
- Entidades em `Prensadao.Domain/Entities` devem proteger invariantes do domínio com métodos próprios quando a regra pertencer ao estado da entidade.
- Interfaces de repositório ficam no Domain; implementações concretas ficam em `Prensadao.Infra/Persistence/Repositories`.
- Registre novas dependências nos módulos corretos: serviços em `ApplicationModule`, repositórios, workers, mensageria, banco e integrações em `InfrastructureModule`.
- Evite dependências circulares. A direção esperada é API -> Application/Infra, Application -> Domain, Infra -> Application/Domain. Quando não houver alternativa simples sem quebrar a arquitetura, use `IServiceProvider` como último recurso, de forma pontual, documentada e limitada ao ponto de composição necessário.

## Estilo de Código C#
- Use indentação de 4 espaços.
- Use `PascalCase` para tipos, métodos, propriedades e membros públicos.
- Use `camelCase` para variáveis locais e parâmetros.
- Use `_camelCase` para campos privados somente leitura.
- Mantenha uma classe principal por arquivo e faça o arquivo corresponder ao tipo principal.
- Prefira injeção via construtor.
- Use `async`/`await` em operações de I/O e retorne `Task`/`Task<T>` nos contratos assíncronos.
- Não use `async void`.
- Use `ArgumentNullException.ThrowIfNull` para entradas obrigatórias quando fizer sentido.
- Prefira mensagens de exceção claras e específicas para regras de negócio.
- Evite comentários óbvios. Comente apenas decisões não triviais, integrações ou regras que não sejam evidentes pelo código.

## DTOs, Contratos e Mapeamento
- DTOs de entrada ficam em `Prensadao.Application/DTOs/Requests`.
- DTOs de saída ficam em `Prensadao.Application/DTOs/Responses`.
- Use sufixos descritivos como `RequestDto`, `ResponseDto` e `Dto`. Mantenha o padrão já existente quando editar tipos legados.
- Não exponha entidades de domínio diretamente nos controllers quando já houver DTO correspondente.
- Ao adicionar campo em entidade, verifique DTOs, mapeamentos, configurações EF, testes e payloads de API relacionados.

## Regras de Domínio e Aplicação
- Validações de negócio devem ficar nos serviços ou nas entidades, não nos repositórios.
- Repositórios devem persistir e consultar dados, sem publicar mensagens ou executar regra de negócio.
- Ao alterar criação ou atualização de pedidos, produtos ou clientes, preserve validações de ids, duplicidade, valores positivos, status e itens obrigatórios.
- Ao alterar status de pedido, revise impactos em notificação, cancelamento, mensageria e testes.
- Ao alterar promoções com IA, valide limites de desconto, existência do produto sugerido, preço promocional e tratamento de respostas inválidas.

## Persistência e EF Core
- Configurações de entidades devem ficar em `Prensadao.Infra/Persistence/Configurations`.
- Use migrations apenas quando houver mudança real no modelo persistido.
- Não edite migrations antigas manualmente, salvo correção explícita e justificada. Prefira nova migration.
- Ao adicionar entidade ou relacionamento, atualize `PrensadaoDbContext`, configuração EF, interface de repositório, implementação e registro de DI.
- Use consultas com `Include` apenas quando a tela/serviço precisar dos detalhes. Evite carregar grafos desnecessários.
- Preserve tipos adequados para dinheiro (`decimal`) e datas conforme o padrão atual com NodaTime/UTC.

## Mensageria, Workers e Hangfire
- Constantes de filas, exchanges e routing keys devem ficar centralizadas em `RabbitMqConstants` ou no local já estabelecido.
- Publicação de mensagens deve passar por `IMessagePublisher`.
- Consumo deve passar por `IConsumer` e workers em `Prensadao.Infra/Messaging/Workers`.
- Mensagens devem ser serializadas em JSON e manter contratos compatíveis. Ao alterar `OrderMessageDto` ou `NotifyMessageDto`, atualize produtores, consumidores e testes.
- Workers devem tratar cancelamento e falhas de forma previsível, sem bloquear o host indefinidamente.

## Controllers e API
- Mantenha rotas e verbos HTTP consistentes com os controllers existentes.
- Para endpoints novos, retorne códigos HTTP compatíveis com o resultado: `200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound` ou `500` quando aplicável.
- Não vaze detalhes sensíveis de exceções para o cliente.
- Se ativar ou alterar middleware de erro/correlação, revise todos os controllers que fazem `try/catch` local para evitar tratamento duplicado.

## Testes
- Testes usam xUnit, FakeItEasy, AutoFixture e FluentAssertions.
- Coloque novos testes em `Prensadao.Test/<Area>/`.
- Nomeie arquivos de teste conforme o item testado, por exemplo `CustomerServiceTest.cs`.
- Prefira o padrão `Method_ShouldExpectedBehavior_WhenCondition` para novos testes. Em arquivos já existentes com padrão em português, mantenha consistência local.
- Cubra caminhos de sucesso, validações de entrada, regras de exceção e chamadas esperadas a repositórios ou mensageria.
- Ao mudar regras de serviço, contratos de repositório, DTOs de mensagem, processamento de workers ou integração com IA, adicione ou atualize testes.
- Não dependa de PostgreSQL, RabbitMQ ou Azure OpenAI reais em testes unitários. Use fakes/mocks.

## Configuração e Segurança
- Nunca commit segredos, strings de conexão reais, chaves do RabbitMQ ou credenciais do Azure OpenAI.
- Use variáveis de ambiente, user secrets ou configurações locais fora do versionamento para dados sensíveis.
- Trate `appsettings.Development.json` como configuração local de desenvolvimento.
- Não registre payloads sensíveis em logs.
- Ao mexer em Docker, banco, RabbitMQ ou Hangfire, preserve compatibilidade com `docker compose up -d --build`.

## Documentação e Comentários
- Atualize o `README.md` quando mudar comandos de execução, configuração obrigatória, Docker Compose, migrations ou comportamento público relevante da API.
- Adicione exemplos de payload quando criar ou alterar endpoints de forma significativa.
- Comentários no código devem explicar o motivo de uma decisão, não repetir o que a linha faz.

## Git, Commits e Pull Requests
- O assistente pode executar comandos `git` somente quando o usuário pedir explicitamente.
- Antes de criar commits, mostre a separação proposta dos arquivos por commit, sugira mensagens e aguarde confirmação.
- Não execute comandos destrutivos ou de reescrita de histórico sem autorização explícita e específica, incluindo `git reset --hard`, `git checkout --`, `git clean`, `git rebase` e `git push --force`.
- Mantenha commits focados em uma única preocupação.
- Mensagens de commit devem ser curtas, no imperativo, e podem usar prefixos como `refactor:`, `add:`, `fix:` ou frases diretas no estilo do histórico.
- Pull requests devem incluir resumo conciso, projetos impactados, evidências de teste, issue/tarefa vinculada quando houver e exemplos de payload quando a API mudar.

## Conduta do Assistente
- Preserve alterações existentes do usuário. Não reverta mudanças que não foram feitas por você.
- Antes de editar, entenda o contexto local com leitura dos arquivos relacionados.
- Prefira soluções simples e alinhadas ao padrão do repositório.
- Não faça refatorações amplas junto com uma correção pequena, salvo se forem necessárias para completar a tarefa.
- Ao terminar, informe arquivos alterados, testes executados e qualquer limitação encontrada.
