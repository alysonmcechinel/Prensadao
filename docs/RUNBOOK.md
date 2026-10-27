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
