# Roadmap e Pendências

Este documento reúne trabalho ainda não concluído. Itens daqui não devem ser descritos como funcionalidades prontas em `FEATURES.md`.

## Tratamento de Erros

- Seguir a proposta e as decisões pendentes registradas em `ERROR_HANDLING.md`.
- Decidir e aplicar o tratamento global de exceções: `RequestContextMiddleware` existe, mas está comentado em `Prensadao/Program.cs`.
- Remover a captura genérica de `Exception` em `PromotionsController` quando o tratamento global estiver ativo, preservando respostas `ProblemDetails` coerentes.

## Promoções com IA

- Adicionar testes unitários para `PromotionGeneratorService`.
- Adicionar testes para validação e falhas de `AzureOpenAiPromotionService`, sem acessar o serviço real.
- Cobrir o mapeamento de `AiResponseValidationException` no tratamento HTTP escolhido.

## Padronização da Linguagem e dos Contratos

Os itens abaixo registram divergências existentes entre nomes, contratos e comportamentos. Nenhum deles está corrigido. Antes de alterar um contrato HTTP, mensagem ou regra de negócio, defina compatibilidade, migração e cobertura de testes.

| Pendência | Classificação | Resultado esperado |
| --- | --- | --- |
| O endpoint e os métodos `Enabled` de pedido executam cancelamento. | Nomenclatura e contrato HTTP. | Renomear a operação para cancelamento sem confundi-la com ativação de produto. |
| `AdvanceStatus` segue o fluxo, enquanto `SetStatus` permite atribuição direta. | Decisão de regra de negócio. | Definir se saltos de status são permitidos e tornar cada operação explícita no contrato. |
| `GenerateWeeklyPromotionAsync` usa janelas de 360 e 84 dias. | Nomenclatura e regra de análise. | Renomear o caso de uso ou redefinir as janelas depois de decidir o significado de “semanal”. |
| As análises agrupam `Order.CreatedAt.DayOfWeek` em UTC, embora o dia comercial siga `America/Sao_Paulo`. | Possível defeito funcional na análise temporal. | Converter limites e agrupamentos para o fuso comercial antes de determinar data e dia da semana, cobrindo a virada do dia em testes. |
| Um produto inativo pode ser elegível para promoção. | Decisão de regra de negócio. | Decidir se `Enabled` participa da elegibilidade e aplicar a decisão na consulta e nos testes. |
| Pedidos cancelados participam das métricas de vendas. | Decisão de regra de negócio. | Definir quais status compõem vendas e receita e aplicar o mesmo critério às análises. |
| “Notificação” atualmente significa gerar texto e escrevê-lo no console. | Nomenclatura e capacidade futura. | Manter o nome limitado à geração atual ou criar uma operação distinta quando houver envio externo. |
| `Error = 99` existe no enum, mas status desconhecido não é convertido automaticamente para esse valor. | Modelagem de status e fallback. | Definir se `Error` é estado real, fallback de notificação ou conceito a ser removido. |

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
