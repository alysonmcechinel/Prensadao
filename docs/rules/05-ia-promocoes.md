# Rules: IA de Promoções

## Regra
- O controller de promoções deve chamar `IPromotionGenerator`; não deve chamar Azure OpenAI diretamente.
- A integração com Azure OpenAI fica atrás de `IPromotionSuggestionService`.
- A IA deve receber apenas produtos elegíveis e regras explícitas de desconto.
- A resposta da IA deve ser JSON válido no schema esperado.
- Valide `ProductId`, `PromotionName` e `DiscountPercent` antes de usar a resposta.
- Nunca aceite produto fora da lista elegível.
- Nunca aceite desconto fora dos limites definidos pela request.
- Não registre prompts com dados sensíveis ou chaves de API.
- Não versione `ApiKey` real em `appsettings.json`.
- Ao alterar prompt, schema, DTOs ou validações de IA, adicione ou atualize testes.
- Antes de evoluir a feature, resolva ou documente conscientemente o middleware de erro comentado em `Program.cs`.

## Exemplo Bom
Controller depende do caso de uso, não do provider de IA.

```csharp
public PromotionsController(IPromotionGenerator promotionGenerator)
{
    _promotionGenerator = promotionGenerator;
}
```

Validação da resposta antes de usar a sugestão.

```csharp
if (!int.TryParse(suggestion.ProductId, out var suggestedProductId))
    throw new AiResponseValidationException("ProductId deve ser numérico para este domínio.");

if (!topProducts.Any(product => product.ProductId == suggestedProductId))
    throw new AiResponseValidationException("ProductId sugerido não pertence aos produtos elegíveis.");

if (suggestion.DiscountPercent < minDiscountPercent || suggestion.DiscountPercent > maxDiscountPercent)
    throw new AiResponseValidationException("DiscountPercent fora dos limites definidos.");
```

Prompt com limites explícitos.

```csharp
promptBuilder.AppendLine("Produtos elegíveis para promoção (use apenas um deles):");
promptBuilder.AppendLine($"Regra de desconto: mínimo {minDiscountPercent}% e máximo {maxDiscountPercent}%.");
promptBuilder.AppendLine("Não invente produto fora da lista e não gere preço negativo.");
```

## Evite
Aceitar a saída da IA sem validação.

```csharp
var suggestion = await _promotionSuggestionService.SuggestPromotionAsync(products, day, 5, 25, token);
var product = await _productRepository.GetByIdAsync(int.Parse(suggestion.ProductId));

return new PromotionProductDto
{
    ProductId = product.Id,
    DiscountPercent = suggestion.DiscountPercent
};
```

## Por quê?
IA é entrada externa e não confiável. Mesmo com schema e prompt forte, o sistema precisa validar o resultado antes de aplicar regra de negócio.
