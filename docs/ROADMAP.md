# Roadmap e Pendências

Este documento reúne trabalho ainda não concluído. Itens daqui não devem ser descritos como funcionalidades prontas em `FEATURES.md`.

## Tratamento de Erros

- Decidir e aplicar o tratamento global de exceções: `RequestContextMiddleware` existe, mas está comentado em `Prensadao/Program.cs`.
- Remover a captura genérica de `Exception` em `PromotionsController` quando o tratamento global estiver ativo, preservando respostas `ProblemDetails` coerentes.

## Promoções com IA

- Adicionar testes unitários para `PromotionGeneratorService`.
- Adicionar testes para validação e falhas de `AzureOpenAiPromotionService`, sem acessar o serviço real.
- Cobrir o mapeamento de `AiResponseValidationException` no tratamento HTTP escolhido.

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
