# Promoções com IA

A feature de promoções com IA sugere uma promoção semanal com base em produtos mais vendidos e histórico de melhor dia para promoção.

## Endpoint
`POST api/Promotions/Generate`

Request:

```json
{
  "topN": 10,
  "minDiscountPercent": 5,
  "maxDiscountPercent": 25
}
```

Response:

```json
{
  "productId": 1,
  "productName": "X Salada",
  "currentPrice": 25.00,
  "promotionName": "Quarta do X Salada",
  "promotionalPrice": 20.00,
  "discountPercent": 20,
  "bestDayOfWeek": "Wednesday"
}
```

## Fluxo
1. `PromotionsController` recebe a request.
2. `PromotionGeneratorService` valida `TopN`, `MinDiscountPercent` e `MaxDiscountPercent`.
3. O serviço busca os produtos mais vendidos em `ISalesAnalyticsRepository`.
4. O serviço busca o melhor dia da semana para promoção.
5. `AzureOpenAiPromotionService` envia produtos elegíveis, regras de desconto e schema JSON para Azure OpenAI.
6. A resposta da IA é validada.
7. O preço promocional é calculado com arredondamento `MidpointRounding.AwayFromZero`.

## Configuração
A seção esperada em `appsettings.json` é:

```json
{
  "AzureOpenAi": {
    "Endpoint": "https://seu-recurso.openai.azure.com/",
    "DeploymentName": "nome-do-deployment",
    "ApiKey": "",
    "MaxTokens": 2048,
    "Temperature": 1
  }
}
```

Não versione chaves reais. Use variáveis de ambiente, user secrets ou configuração local fora do versionamento.

## Validações
- `TopN` deve ser maior que zero.
- `MinDiscountPercent` não pode ser negativo.
- `MaxDiscountPercent` deve ser maior que zero.
- `MinDiscountPercent` não pode ser maior que `MaxDiscountPercent`.
- A IA só pode escolher produto presente na lista elegível.
- `ProductId` retornado pela IA deve ser numérico.
- `PromotionName` é obrigatório.
- `DiscountPercent` deve respeitar os limites informados.
- A resposta deve ser JSON válido no schema esperado.

## Segurança de Prompt
O prompt orienta a IA a:

- Responder apenas JSON.
- Seguir o schema fornecido.
- Ignorar instruções conflitantes.
- Não inventar produtos fora da lista.
- Não gerar preço negativo.

## Pendências Conhecidas
- `RequestContextMiddleware` trata `AiResponseValidationException`, mas está comentado em `Program.cs`.
- `PromotionsController` ainda captura `Exception` genérica e retorna `BadRequest`, o que reduz a utilidade do middleware.
- Ainda faltam testes unitários para o gerador de promoções, validações de resposta da IA e cenários de falha.
