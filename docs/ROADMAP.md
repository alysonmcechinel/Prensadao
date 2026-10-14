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
| `OrderItemResponseDto.ProductValue` recebe o preço atual do produto, não o `UnitPrice` histórico do item. | Possível defeito funcional e contrato HTTP. | Decidir quais preços a resposta deve expor e nomear separadamente preço histórico e preço atual. |
| O domínio usa `Notes`, enquanto os contratos usam `Observation`. | Nomenclatura entre camadas. | Escolher o termo canônico por limite e manter o mapeamento explícito enquanto houver nomes diferentes. |
| O domínio usa `CreatedAt`, enquanto a resposta usa `DateOrder` e converte o valor para horário local. | Nomenclatura e semântica temporal do contrato. | Tornar nome, fuso e tipo temporal inequívocos sem reinterpretar dados existentes. |
| O domínio usa `Status`, enquanto os contratos usam `OrderStatus` em formatos diferentes. | Nomenclatura e contrato HTTP. | Padronizar o significado e documentar claramente os formatos de entrada e saída. |
| O domínio usa `IsDelivery`, enquanto os contratos usam `Delivery`. | Nomenclatura entre camadas. | Escolher nomes coerentes para o booleano e preservar compatibilidade quando necessário. |
| `NotifyMessageDto.ConsumerName` contém o nome do cliente. | Nomenclatura e contrato de mensagem. | Renomear para `CustomerName` com estratégia de compatibilidade entre produtor e consumidor. |
| O endpoint e os métodos `Enabled` de pedido executam cancelamento. | Nomenclatura e contrato HTTP. | Renomear a operação para cancelamento sem confundi-la com ativação de produto. |
| `AdvanceStatus` segue o fluxo, enquanto `SetStatus` permite atribuição direta. | Decisão de regra de negócio. | Definir se saltos de status são permitidos e tornar cada operação explícita no contrato. |
| `GenerateWeeklyPromotionAsync` usa janelas de 360 e 84 dias. | Nomenclatura e regra de análise. | Renomear o caso de uso ou redefinir as janelas depois de decidir o significado de “semanal”. |
| Um produto inativo pode ser elegível para promoção. | Decisão de regra de negócio. | Decidir se `Enabled` participa da elegibilidade e aplicar a decisão na consulta e nos testes. |
| Pedidos cancelados participam das métricas de vendas. | Decisão de regra de negócio. | Definir quais status compõem vendas e receita e aplicar o mesmo critério às análises. |
| “Notificação” atualmente significa gerar texto e escrevê-lo no console. | Nomenclatura e capacidade futura. | Manter o nome limitado à geração atual ou criar uma operação distinta quando houver envio externo. |
| `Error = 99` existe no enum, mas status desconhecido não é convertido automaticamente para esse valor. | Modelagem de status e fallback. | Definir se `Error` é estado real, fallback de notificação ou conceito a ser removido. |

## Critério de Manutenção

Ao concluir um item, remova-o deste arquivo e atualize a documentação do comportamento implementado. Novas pendências devem indicar o resultado esperado, sem antecipar que a funcionalidade já existe.
