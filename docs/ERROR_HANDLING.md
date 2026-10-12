# Plano de Padronização de Erros HTTP

## Status

Este documento descreve uma proposta ainda não implementada. O comportamento atual continua registrado em `ARCHITECTURE.md`, `FEATURES.md` e `ROADMAP.md`.

O objetivo da próxima implementação é substituir respostas de erro inconsistentes por um contrato HTTP previsível baseado em `ProblemDetails`, sem colocar regra de negócio nos controllers nem acoplar a API diretamente ao Azure OpenAI.

## Crítica ao Estado Atual

- `RequestContextMiddleware` concentra correlação, medição de tempo, log e tradução de exceções. Ele possui mais de um motivo para mudar.
- O middleware está comentado em `Prensadao/Program.cs` e, portanto, não participa do pipeline.
- Os controllers capturam `Exception` e normalmente devolvem `400 Bad Request`, mesmo quando a falha representa recurso inexistente, indisponibilidade externa ou erro interno.
- A resposta varia entre texto simples e `ProblemDetails`, apesar de alguns endpoints anunciarem `ProblemDetails` em `ProducesResponseType`.
- `RequestContextMiddleware` conhece `Azure.RequestFailedException`. Isso acopla o tratamento HTTP ao SDK de um provedor específico.
- Exceções genéricas como `ArgumentException` e `InvalidOperationException` representam situações diferentes e não fornecem semântica suficiente para um mapeamento HTTP seguro.

Não se deve selecionar o status HTTP procurando palavras em `Exception.Message`. Mensagens podem mudar; o tipo da falha deve carregar a sua semântica.

## Alternativas Avaliadas

### Alternativa A: `IExceptionHandler` e exceções tipadas

Usar o pipeline nativo do ASP.NET Core 8 com `AddProblemDetails()`, `AddExceptionHandler<T>()` e `UseExceptionHandler()`.

Vantagens:

- centraliza a tradução de exceções para HTTP;
- mantém controllers finos;
- produz um contrato compatível com `ProblemDetails`;
- permite evolução incremental do código atual;
- separa correlação, regra de negócio, integração externa e resposta HTTP.

Desvantagens:

- exige definir exceções com significados claros;
- o fluxo de falha não aparece no tipo de retorno dos services.

### Alternativa B: `Result<T>`

Fazer os services retornarem sucesso ou uma falha tipada, em vez de usar exceções para erros esperados.

Vantagens:

- falhas esperadas ficam explícitas no contrato do service;
- reduz o uso de exceções como controle de fluxo;
- favorece composição de validações.

Desvantagens:

- altera interfaces, services e controllers existentes;
- introduz uma abstração nova em grande parte do projeto;
- aumenta o escopo da mudança antes de o contrato HTTP estar estabilizado.

## Decisão Recomendada

Adotar a alternativa A. Ela resolve o problema atual com menor impacto e usa recursos nativos do ASP.NET Core 8.

`Result<T>` pode ser reavaliado futuramente se a quantidade de falhas esperadas tornar os services difíceis de compreender. Não implementar as duas alternativas simultaneamente.

## Divisão de Responsabilidades

### API

- `RequestContextMiddleware` cria ou propaga o correlation ID e registra a duração da requisição.
- `GlobalExceptionHandler` converte exceções conhecidas em status HTTP e `ProblemDetails`.
- Controllers descrevem contratos HTTP, chamam services e devolvem respostas de sucesso.
- O fallback de erro inesperado não expõe stack trace, credenciais ou detalhes internos.

Como esses componentes tratam exclusivamente do pipeline HTTP, a localização preferida é o projeto `Prensadao/`, e não `Prensadao.Infra/`.

### Application

- Services validam e executam casos de uso.
- Falhas conhecidas usam exceções tipadas com significado independente de HTTP.
- `AiResponseValidationException` permanece uma falha da validação da resposta da IA.
- A Application não escolhe status HTTP nem cria `ProblemDetails`.

### Infra

- Integrações capturam exceções específicas de seus SDKs quando for necessário acrescentar contexto ou impedir vazamento do provedor.
- Uma falha do SDK do Azure deve ser traduzida para uma exceção independente do provedor, definida em Application, antes de chegar à API.
- Repositories continuam responsáveis apenas por persistência e consultas.

### Domain

- Entidades protegem invariantes do próprio estado.
- O Domain não conhece HTTP, `ProblemDetails`, middleware ou SDKs externos.

## Contrato de Erro

Todas as respostas tratadas devem usar `Content-Type: application/problem+json` e manter os campos padrão de `ProblemDetails`.

Exemplo:

```json
{
  "type": "https://prensadao/errors/resource-not-found",
  "title": "Recurso não encontrado",
  "status": 404,
  "detail": "Pedido não encontrado.",
  "instance": "/api/Order/GetById?id=123",
  "code": "RESOURCE_NOT_FOUND",
  "correlationId": "0f49e137c5464807a9ac6fd45ba80187"
}
```

Campos:

| Campo | Responsabilidade |
| --- | --- |
| `type` | Identificador estável e documentável do tipo de problema |
| `title` | Resumo seguro e legível |
| `status` | Status HTTP da resposta |
| `detail` | Explicação segura da ocorrência; não deve conter detalhes internos |
| `instance` | Caminho da requisição que falhou |
| `code` | Código estável para clientes da API |
| `correlationId` | Identificador que liga resposta e logs |

`code` deve ser estável. O frontend não deve tomar decisões comparando o texto de `detail`.

## Mapeamento Inicial Proposto

Os nomes finais das novas exceções devem ser confirmados antes da implementação. O significado do mapeamento é mais importante do que o nome exato da classe.

| Falha da aplicação | HTTP | `code` | Observação |
| --- | ---: | --- | --- |
| Request inválida | 400 | `VALIDATION_ERROR` | Campo, id, quantidade ou faixa inválida |
| Recurso inexistente | 404 | `RESOURCE_NOT_FOUND` | Cliente, produto ou pedido não encontrado |
| Conflito com estado atual | 409 | `CONFLICT` | Duplicidade ou transição incompatível |
| Resposta inválida da IA | 422 | `AI_RESPONSE_INVALID` | `AiResponseValidationException` |
| Dados insuficientes para promoção | 422 | `INSUFFICIENT_SALES_DATA` | Request válida, mas impossível de processar com os dados atuais |
| Provedor externo indisponível | 502 | `EXTERNAL_PROVIDER_ERROR` | Falha traduzida pela Infra; sem expor o SDK do Azure |
| Falha inesperada | 500 | `UNEXPECTED_ERROR` | Mensagem genérica para o cliente e detalhes somente no log |

Erros automáticos de model binding e validação do `[ApiController]` também devem ser revisados para manter formato e extensões coerentes com esse contrato.

## Estrutura Inicial Sugerida

Os caminhos abaixo são uma proposta e podem ser ajustados durante o estudo:

```text
Prensadao/
├── ErrorHandling/
│   └── GlobalExceptionHandler.cs
└── Middleware/
    └── RequestContextMiddleware.cs

Prensadao.Application/
└── Exceptions/
    ├── AiResponseValidationException.cs
    ├── RequestValidationException.cs
    ├── ResourceNotFoundException.cs
    ├── ConflictException.cs
    ├── InsufficientSalesDataException.cs
    └── ExternalProviderException.cs
```

Evitar uma hierarquia extensa de exceções antes de existir necessidade real. Uma classe deve representar uma categoria que altera o tratamento, o status, o código ou a política de log.

## Ordem de Implementação

1. Confirmar o contrato de erro e o mapeamento da tabela anterior.
2. Adicionar as exceções mínimas necessárias em Application.
3. Fazer a Infra traduzir falhas do Azure para uma exceção independente do provedor.
4. Separar correlação/tempo de tratamento de exceções.
5. Implementar `GlobalExceptionHandler` com `IExceptionHandler` e `IProblemDetailsService`.
6. Registrar `AddProblemDetails()`, `AddExceptionHandler<GlobalExceptionHandler>()` e `UseExceptionHandler()` no ponto de composição.
7. Remover gradualmente os `catch (Exception)` dos controllers.
8. Substituir exceções genéricas por exceções tipadas nos fluxos alterados.
9. Revisar `ProducesResponseType` no Swagger.
10. Atualizar `ARCHITECTURE.md`, `FEATURES.md`, `ROADMAP.md` e este documento somente depois que o comportamento estiver implementado.

Começar por `PromotionsController` reduz o escopo e cobre os casos `400`, `422`, `502` e `500`. Depois de validar o padrão, aplicá-lo aos controllers de pedidos, produtos e clientes.

## Testes Necessários

### Testes do handler HTTP

- retorna `400` e `VALIDATION_ERROR` para request inválida;
- retorna `404` e `RESOURCE_NOT_FOUND` para recurso inexistente;
- retorna `409` e `CONFLICT` para conflito;
- retorna `422` e `AI_RESPONSE_INVALID` para resposta inválida da IA;
- retorna `502` sem detalhes do Azure para falha do provedor;
- retorna `500` com mensagem segura para exceção desconhecida;
- inclui `correlationId`, `instance` e `application/problem+json`;
- preserva um `X-Correlation-Id` recebido ou cria um quando ausente.

### Testes dos controllers

- confirmam que o caminho de sucesso permanece inalterado;
- confirmam que os controllers não convertem novamente as exceções;
- não acessam PostgreSQL, RabbitMQ ou Azure OpenAI reais.

Preferir poucos testes de integração do pipeline HTTP para comprovar middleware, handler e serialização juntos. Manter testes unitários para regras dos services e tradução do provider.

Ao finalizar o código, executar:

```powershell
dotnet build Prensadao.sln
dotnet test Prensadao.Test/Prensadao.Test.csproj
```

## Decisões a Confirmar Antes de Codificar

- Os nomes das exceções tipadas e seus códigos estáveis.
- Se duplicidade de telefone/nome será `409 Conflict` em todos os casos.
- Se ausência de vendas para promoção permanecerá `422 Unprocessable Entity`.
- O formato dos erros automáticos produzidos pelo `[ApiController]`.
- A estratégia de teste HTTP: `WebApplicationFactory` ou teste direto do handler seguido por um teste de integração mínimo.

## Referências

- [Tratamento de erros em APIs ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api)
- [`IExceptionHandler`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler)
- [`ProblemDetails`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.problemdetails)
- `docs/rules/00-principios.md`
- `docs/rules/01-arquitetura.md`
- `docs/rules/03-testes.md`
