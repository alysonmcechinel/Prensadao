# Tratamento de Erros HTTP

A API usa `IExceptionHandler` do ASP.NET Core 8 para tratar exceções que escapam das controllers.

## Responsabilidades

- `Prensadao/ErrorHandling/GlobalExceptionHandler.cs`: classifica a falha, registra a exceção original e devolve `ProblemDetails`.
- `Prensadao/Extensions/ErrorHandlingExtensions.cs`: registra o tratamento e filtra a categoria de logs do middleware nativo para evitar duplicação no .NET 8. Esse filtro desabilita todos os logs dessa categoria; não desabilita o tratamento de exceções.
- `Prensadao/Program.cs`: ativa `UseExceptionHandler()` antes dos endpoints.
- Application: distingue falhas esperadas com exceções específicas, sem conhecer HTTP. Controllers continuam responsáveis apenas pelo contrato HTTP e pelas chamadas aos serviços.

## Respostas

| Exceção | HTTP |
| --- | ---: |
| `RequestValidationException` | 400 |
| `ResourceNotFoundException` | 404 |
| `ConflictException` | 409 |
| `AiResponseValidationException` | 422 |
| `InsufficientSalesDataException` | 422 |
| Qualquer outra exceção, incluindo transação abortada | 500 |

O handler devolve JSON com `status`, `title`, `detail`, `instance` e `traceId`. Para `500`, a mensagem é genérica; detalhes técnicos ficam no log. O identificador vem de `HttpContext.TraceIdentifier`, disponível sem middleware próprio, e permite associar a resposta ao registro da exceção. Ele identifica a requisição e não representa um trace distribuído.

Validações automáticas do MVC, como JSON malformado e campos obrigatórios ausentes, continuam usando o comportamento nativo de `ValidationProblemDetails`, com `application/json` conforme o atributo `Produces` das controllers. O handler escreve exceções como `application/problem+json`. Não há customização de model binding.

## Logs e testes

O handler registra a exceção completa com método, caminho, status e identificador da requisição. Falhas esperadas usam `Warning`; falhas inesperadas usam `Error`. Não são acrescentados corpos de requests, prompts ou credenciais. Avisos e erros de comandos EF Core permanecem habilitados, sem ativar `EnableSensitiveDataLogging`.

`Prensadao.Test/Api/ErrorHandlingTest.cs` exercita o pipeline com TestServer e serviços falsos: mapeamento HTTP, resposta segura, identificador no log, ausência de duplicação, validação nativa e preservação do sucesso. Não depende de PostgreSQL, RabbitMQ ou Azure reais.

Correlação por headers, logs JSON, métricas por requisição, exportação de traces, tradução específica de falhas do SDK e coleta automática de logs pelo k6 ficam fora desta etapa. Relatórios HTML existentes do k6 continuam disponíveis.

O tratamento não altera transações, persistência ou mensageria e não corrige a causa das transações abortadas. Falhas de workers ficam fora do pipeline HTTP. Consulte `RUNBOOK.md` para acompanhar os logs durante um teste.
