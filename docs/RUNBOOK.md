# Runbook

Comandos devem ser executados a partir da raiz do repositório.

## Restaurar, Compilar e Testar
```powershell
dotnet restore Prensadao.sln
dotnet build Prensadao.sln
dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"
```

Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`. Para mudanças em regras de negócio, DTOs, contratos públicos, repositórios, mensageria, workers ou IA, rode também os testes relevantes.

## Integração Contínua e Pull Requests

O workflow `.github/workflows/ci.yml` executa em pull requests destinados a `master` e depois de merges nessa branch. O check público `CI / build-and-test`:

1. restaura as dependências da solução;
2. compila a solução em `Release`;
3. executa os testes de `Prensadao.Test`.

O workflow usa somente permissão de leitura do conteúdo, não recebe segredos e não inicia PostgreSQL, RabbitMQ, Hangfire, Azure OpenAI ou a API. Ele valida compilação e testes automatizados; não substitui testes de integração nem comprova que a aplicação completa inicia com a infraestrutura real.

Warnings de compilação permanecem visíveis no log, mas ainda não bloqueiam o PR. O template `.github/pull_request_template.md` registra contexto, validação, impactos, riscos e documentação de cada mudança.

O ruleset ativo de `master` aplica:

- pull request obrigatório, sem aprovação humana obrigatória enquanto houver apenas um mantenedor;
- check `CI / build-and-test` obrigatório e branch atualizada antes do merge;
- resolução obrigatória das conversas;
- bloqueio de push direto, force push e exclusão, sem bypass do proprietário;
- aprovação manual para executar workflows originados de qualquer fork externo.

## Executar API
```powershell
dotnet run --project Prensadao/Prensadao.API.csproj
```

Em desenvolvimento, Swagger é habilitado automaticamente pelo ambiente `Development`. No perfil HTTP local, a API fica disponível em `http://localhost:5185`.

## Docker Compose
```powershell
docker compose up -d --build
```

O compose sobe:

- PostgreSQL em `localhost:5432`.
- RabbitMQ em `localhost:5672`.
- RabbitMQ Management em `localhost:15672`.
- Serviço `api`, que hospeda API, workers e Hangfire, em `http://localhost:8080`.
- API configurada para acessar PostgreSQL e RabbitMQ pelos nomes dos serviços na rede interna.

Validar a configuração resolvida sem iniciar os containers:

```powershell
docker compose config
```

PostgreSQL e RabbitMQ possuem healthchecks. O serviço `api` inicia somente depois que ambos estiverem saudáveis.

### Ambiente isolado para testes de desempenho

O `docker-compose.performance.yml` complementa o compose principal sem duplicar a definição da aplicação. Ele usa nomes de containers próprios, não publica PostgreSQL nem RabbitMQ no host e publica a API de performance em `http://localhost:18080`.

Use sempre o nome de projeto `prensadao-performance`; ele isola rede e volumes dos recursos usados no desenvolvimento:

```powershell
docker compose -p prensadao-performance -f docker-compose.yml -f docker-compose.performance.yml up -d --build
```

Remover o ambiente e seus dados descartáveis:

```powershell
docker compose -p prensadao-performance -f docker-compose.yml -f docker-compose.performance.yml down --volumes --remove-orphans
```

O ambiente de performance ainda não aplica migrations nem executa o k6 automaticamente. Essas responsabilidades pertencem ao runner de performance, que deverá preservar os relatórios antes de remover os volumes.

## Migrations
Aplicar migrations:

```powershell
dotnet ef database update --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj
```

Criar migration quando o modelo persistido mudar:

```powershell
dotnet ef migrations add NomeDaMigration --project Prensadao.Infra/Prensadao.Infra.csproj --startup-project Prensadao/Prensadao.API.csproj --output-dir Persistence/Migrations
```

Não edite migrations antigas sem pedido explícito.

## Configuração Local
- `ConnectionStrings:DefaultConnection` aponta para PostgreSQL.
- `RabbitMq` configura `Host`, `Port`, `User` e `Password`.
- `AzureOpenAi` configura endpoint, deployment, chave, tokens e temperatura.
- Não versione segredos reais.
- Prefira variáveis de ambiente, user secrets ou configuração local fora do versionamento.

O `docker-compose.yml`, o `Dockerfile` e o `.dockerignore` fazem parte da configuração pública do projeto e devem permanecer versionados. O `.gitignore` impede que arquivos locais como `.env` sejam enviados ao repositório, enquanto o `.dockerignore` impede que configurações locais, artefatos de build e metadados do repositório entrem no contexto de build ou na imagem Docker.

Os valores locais do RabbitMQ são `localhost`, porta `5672` e credenciais de desenvolvimento `guest/guest`. Em Docker, o compose sobrescreve o host com `RabbitMq__Host=rabbitmq`. Outros ambientes devem fornecer valores próprios por configuração segura.

## Troubleshooting
- Se a API não conectar no banco, verifique se o PostgreSQL está ativo e se a connection string aponta para o host correto.
- Se a aplicação não iniciar por configuração do RabbitMQ, verifique `RabbitMq:Host`, `Port`, `User` e `Password`. Host, usuário e senha são obrigatórios, e a porta deve estar entre `1` e `65535`.
- Se mensagens não forem consumidas, verifique RabbitMQ, exchanges, filas e workers.
- Se uma mensagem falhar, consulte `prensado.orders.cozinha.dlq` ou `prensado.order-status-messages.dlq` no RabbitMQ Management. Os headers `x-death` indicam a fila de origem e o motivo do dead-lettering.
- Não republique mensagens da DLQ sem corrigir e validar a causa da falha. O sistema não executa retry nem reprocessamento automático da DLQ.
- Se promoções com IA falharem, verifique `AzureOpenAi:Endpoint`, `DeploymentName`, `ApiKey` e se a resposta respeita o schema.
- Se erros de IA não voltarem como `ProblemDetails`, verifique se `RequestContextMiddleware` está habilitado no pipeline.

## Migração das Filas para Dead-letter

O RabbitMQ não permite redeclarar uma fila existente com novos argumentos. Na primeira execução desta versão, filas criadas anteriormente sem `x-dead-letter-exchange` podem causar `PRECONDITION_FAILED`.

Antes da atualização em um ambiente com filas existentes:

1. Pare os produtores e consumidores.
2. Verifique se `prensado.orders.cozinha` e `prensado.order-status-messages` possuem mensagens pendentes e preserve ou processe essas mensagens antes de prosseguir.
3. Remova somente essas duas filas principais pelo RabbitMQ Management; essa operação elimina qualquer mensagem que ainda estiver nelas.
4. Inicie a aplicação para que `RabbitMqStartup` recrie as filas com os argumentos de dead-letter e declare as respectivas DLQs.
5. Confirme no RabbitMQ Management os bindings com `prensado.dead-letter` antes de reativar os produtores.

Em uma instalação nova, nenhuma ação de migração é necessária.
