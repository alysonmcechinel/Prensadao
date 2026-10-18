# Promoções com IA

A feature de promoções com IA gera uma sugestão para um produto com base nos produtos mais vendidos e no histórico usado para selecionar o melhor dia da semana.

O caso de uso `GeneratePromotionAsync` é executado sob demanda. O nome não representa frequência, validade ou agendamento semanal; o aspecto semanal da análise é somente a escolha de `BestDayOfWeek`.

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

## Análises e Cálculos Atuais

As regras abaixo descrevem o comportamento implementado. As decisões ainda pendentes sobre nomenclatura, status considerados e elegibilidade de produtos estão registradas em `ROADMAP.md`.

### Produtos mais vendidos

- O intervalo contém os últimos `TopProductsLookbackDays` dias comerciais completos (atualmente 360), considerando `America/Sao_Paulo` e excluindo o dia comercial atual.
- O início corresponde à meia-noite local do primeiro dia incluído e o fim exclusivo corresponde à meia-noite local do dia atual; ambos são convertidos para UTC antes da consulta.
- Os itens de pedido são agrupados por produto, nome e preço atual.
- `UnitsSold` é a soma de `OrderItem.Quantity` para o produto.
- `Revenue` é a soma de `Quantity * UnitPrice`, usando o preço histórico de cada item.
- A classificação usa `UnitsSold` em ordem decrescente e, em empate, `Revenue` em ordem decrescente.
- Apenas os primeiros `TopN` produtos são retornados e enviados como candidatos à IA.
- A consulta atual não filtra `Order.Status` nem `Product.Enabled`.
- Se nenhum produto for encontrado, a geração termina com erro antes de chamar a IA.

### Melhor dia da semana

- O intervalo contém os últimos `BestDayLookbackDays` dias comerciais completos (atualmente 84), considerando `America/Sao_Paulo` e excluindo o dia comercial atual.
- O início corresponde à meia-noite local do primeiro dia incluído e o fim exclusivo corresponde à meia-noite local do dia atual; ambos são convertidos para UTC antes da consulta.
- Os itens são agrupados pelo `DayOfWeek` local de `Order.CreatedAt` em `America/Sao_Paulo`.
- Para cada dia da semana, o sistema calcula a receita total, as unidades totais e a quantidade de datas comerciais distintas presentes no grupo.
- A receita média do dia é `receita total / quantidade de datas distintas`.
- A classificação usa a receita média em ordem decrescente e, em empate, as unidades totais em ordem decrescente.
- Quando não existem dados no intervalo, o resultado padrão é `Friday`.
- O resultado usa o nome em inglês produzido por `DayOfWeek.ToString()`.
- A consulta atual não filtra `Order.Status` nem `Product.Enabled`.

### Preço promocional

- O desconto sugerido é limitado aos valores mínimo e máximo da request com `Math.Clamp`, mesmo após a validação da resposta da IA.
- O preço é calculado por `CurrentPrice * (1 - DiscountPercent / 100)`.
- O resultado é arredondado para duas casas decimais com `MidpointRounding.AwayFromZero`.
- Um preço promocional negativo é rejeitado.

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

## Trabalho Futuro

As pendências de tratamento de erro e cobertura automatizada estão centralizadas em `ROADMAP.md`.
