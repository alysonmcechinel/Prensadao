# Rules: Arquitetura

## Regra
- Preserve a separação entre API, Application, Domain e Infra.
- Não coloque regra de negócio em controllers.
- Não coloque acesso direto a EF Core, RabbitMQ, Hangfire ou Azure OpenAI em controllers.
- Domain não deve depender de Application, Infra ou API.
- Interfaces de repositório ficam no Domain; implementações ficam na Infra.
- Serviços de aplicação orquestram regras, repositórios, mensagens e integrações.
- Registre serviços em `ApplicationModule`.
- Registre infraestrutura em `InfrastructureModule`.
- Use `IServiceProvider` apenas como último recurso no ponto de composição, com escopo limitado.
- Evite refatorações amplas quando a tarefa pedir uma mudança pequena.

## Exemplo Bom
Controller fino: recebe HTTP, chama serviço e devolve resposta.

```csharp
[HttpPost("Generate")]
public async Task<IActionResult> GenerateAsync(
    [FromBody] GeneratePromotionRequest dto,
    CancellationToken cancellationToken)
{
    var result = await _promotionGenerator.GenerateWeeklyPromotionAsync(dto, cancellationToken);
    return Ok(result);
}
```

Serviço de aplicação concentra a regra e a orquestração. Este exemplo é reduzido; mantenha o mapeamento completo do DTO no código real.

```csharp
public async Task<PromotionProductDto> GenerateWeeklyPromotionAsync(
    GeneratePromotionRequest request,
    CancellationToken cancellationToken)
{
    if (request.TopN <= 0)
        throw new ArgumentException("TopN deve ser maior que zero.");

    var topProducts = await _salesAnalyticsRepository.GetTopProductsSoldAsync(
        weekStart,
        nowUtc,
        request.TopN,
        cancellationToken);

    if (topProducts.Count == 0)
        throw new InvalidOperationException("Não há vendas suficientes para gerar promoção.");

    var aiSuggestion = await _promotionSuggestionService.SuggestPromotionAsync(
        TopProductSalesDto.FromModels(topProducts),
        bestDayOfWeek,
        request.MinDiscountPercent,
        request.MaxDiscountPercent,
        cancellationToken);

    return new PromotionProductDto
    {
        ProductId = int.Parse(aiSuggestion.ProductId),
        PromotionName = aiSuggestion.PromotionName,
        DiscountPercent = aiSuggestion.DiscountPercent,
        BestDayOfWeek = bestDayOfWeek
    };
}
```

## Evite
Controller com regra de negócio e acesso direto à infraestrutura.

```csharp
[HttpPost("Generate")]
public async Task<IActionResult> GenerateAsync([FromBody] GeneratePromotionRequest dto)
{
    var products = await _dbContext.Products.ToListAsync();
    var azureClient = new AzureOpenAIClient(endpoint, credential);

    if (dto.TopN <= 0)
        return BadRequest("TopN inválido.");

    // Regra, banco e IA ficaram presos no controller.
    return Ok();
}
```

## Por quê?
Essa separação deixa o projeto mais fácil de estudar, testar e evoluir. Controllers mostram o fluxo HTTP; services mostram os casos de uso; Domain protege conceitos centrais; Infra concentra detalhes externos.
