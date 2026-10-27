# Arquitetura

O Prensadão segue uma separação em camadas para manter controllers finos, regras de negócio isoladas e infraestrutura substituível.

## Camadas
- `Prensadao/`: entrada HTTP da aplicação. Configura controllers, Swagger, JSON com NodaTime, HTTPS, Hangfire Dashboard e rotas.
- `Prensadao.Application/`: casos de uso, validações de fluxo, DTOs, interfaces de serviço e orquestração de repositórios/mensageria/IA.
- `Prensadao.Domain/`: entidades, enums, read models e interfaces de repositório. Não deve depender de API, Application ou Infra.
- `Prensadao.Infra/`: EF Core, PostgreSQL, RabbitMQ, Hangfire, workers, repositórios concretos e Azure OpenAI.
- `Prensadao.Test/`: testes unitários, principalmente dos serviços de aplicação.

## Direção de Dependências
A direção esperada é:

```text
API -> Application
API -> Infra
Application -> Domain
Infra -> Application
Infra -> Domain
```

Não crie dependência de Domain para Application, Infra ou API.

## Composição
- Serviços de aplicação são registrados em `ApplicationModule`.
- Banco, repositórios, RabbitMQ, workers, Hangfire, NodaTime e Azure OpenAI são registrados em `InfrastructureModule`.
- Controllers recebem serviços por injeção de dependência e não instanciam infraestrutura diretamente.

## Responsabilidades
- Controllers validam entrada básica quando necessário, chamam serviços e retornam HTTP responses.
- Services concentram regra de negócio, validação de fluxo e orquestração.
- Entities protegem invariantes ligadas ao próprio estado.
- Repositories persistem e consultam dados; não publicam mensagens nem executam regra de negócio.
- Workers consomem mensagens e executam processamento assíncrono.
- A topologia RabbitMQ associa cada fila principal a uma dead-letter queue própria por meio do exchange direto `prensado.dead-letter`; o consumidor rejeita sem requeue mensagens inválidas ou cujo processamento falha.

## Datas, Dinheiro e Serialização
- Use `decimal` para valores monetários.
- Persista instantes em UTC e preserve o padrão com NodaTime para conversões de fuso horário.
- Casos de uso que dependem do instante atual recebem `IClock` e obtêm UTC pelo helper canônico `_clock.NowUtc()`, permitindo substituir o relógio em testes.
- O contrato de pedido expõe `CreatedAt` como `DateTimeOffset` em `America/Sao_Paulo`, mantendo o offset explícito.
- A API configura serialização JSON com `ConfigureForNodaTime`.
