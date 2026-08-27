# Runbook

Comandos devem ser executados a partir da raiz do repositório.

## Restaurar, Compilar e Testar
```powershell
dotnet restore Prensadao.sln
dotnet build Prensadao.sln
dotnet test Prensadao.Test/Prensadao.Test.csproj --collect:"XPlat Code Coverage"
```

Ao finalizar mudanças de código, rode `dotnet build Prensadao.sln`. Para mudanças em regras de negócio, DTOs, contratos públicos, repositórios, mensageria, workers ou IA, rode também os testes relevantes.

## Executar API
```powershell
dotnet run --project Prensadao/Prensadao.API.csproj
```

Em desenvolvimento, Swagger é habilitado automaticamente pelo ambiente `Development`.

## Docker Compose
```powershell
docker compose up -d --build
```

O compose sobe:

- PostgreSQL em `localhost:5432`.
- RabbitMQ em `localhost:5672`.
- RabbitMQ Management em `localhost:15672`.
- Serviço da API/worker configurado para acessar os serviços pelo network interno.

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
- `AzureOpenAi` configura endpoint, deployment, chave, tokens e temperatura.
- Não versione segredos reais.
- Prefira variáveis de ambiente, user secrets ou configuração local fora do versionamento.

## Troubleshooting
- Se a API não conectar no banco, verifique se o PostgreSQL está ativo e se a connection string aponta para o host correto.
- Se mensagens não forem consumidas, verifique RabbitMQ, exchanges, filas e workers.
- Se promoções com IA falharem, verifique `AzureOpenAi:Endpoint`, `DeploymentName`, `ApiKey` e se a resposta respeita o schema.
- Se erros de IA não voltarem como `ProblemDetails`, verifique se `RequestContextMiddleware` está habilitado no pipeline.
