# Roadmap e Pendências

Este documento reúne trabalho ainda não concluído. Itens daqui não devem ser descritos como funcionalidades prontas em `FEATURES.md`.

## Tratamento de Erros

- Seguir a proposta e as decisões pendentes registradas em `ERROR_HANDLING.md`.
- Decidir e aplicar o tratamento global de exceções: `RequestContextMiddleware` existe, mas está comentado em `Prensadao/Program.cs`.
- Remover a captura genérica de `Exception` em `PromotionsController` quando o tratamento global estiver ativo, preservando respostas `ProblemDetails` coerentes.

## Promoções com IA

- Adicionar testes para validação e falhas de `AzureOpenAiPromotionService`, sem acessar o serviço real.
- Cobrir o mapeamento de `AiResponseValidationException` no tratamento HTTP escolhido.

## Padronização da Linguagem e dos Contratos

Os itens abaixo registram divergências existentes entre nomes, contratos e comportamentos. Nenhum deles está corrigido. Antes de alterar um contrato HTTP, mensagem ou regra de negócio, defina compatibilidade, migração e cobertura de testes.

| Pendência | Classificação | Resultado esperado |
| --- | --- | --- |
| Pedidos cancelados participam das métricas de vendas. | Decisão de regra de negócio. | Definir quais status compõem vendas e receita e aplicar o mesmo critério às análises. |
| “Notificação” atualmente significa gerar texto e escrevê-lo no console. | Nomenclatura e capacidade futura. | Manter o nome limitado à geração atual ou criar uma operação distinta quando houver envio externo. |
| `Error = 99` existe no enum, mas status desconhecido não é convertido automaticamente para esse valor. | Modelagem de status e fallback. | Definir se `Error` é estado real, fallback de notificação ou conceito a ser removido. |

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
