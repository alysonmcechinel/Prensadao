# Glossário do Prensadão

Este documento registra o vocabulário canônico do Prensadão: o significado que palavras e expressões de negócio ou técnicas assumem **dentro deste projeto**. Ele existe para que pessoas, código e ferramentas de IA interpretem os mesmos termos da mesma forma, sem preencher lacunas com significados comuns de outros sistemas de lanchonete, e-commerce ou software.

Os termos abaixo descrevem o modelo atual. Eles não criam funcionalidades e não substituem a documentação das regras de negócio.

## Responsabilidade do documento

| Responsabilidade | Não deve conter |
| --- | --- |
| Definir o significado canônico dos termos usados no Prensadão. | Explicações enciclopédicas de termos técnicos que não possuem significado ou risco de confusão específico no projeto. |
| Delimitar atores, conceitos e ações para evitar frases ambíguas. | Algoritmos, sequências completas de validação ou detalhes internos de implementação. |
| Explicar o que um termo local não significa quando houver risco real de confusão. | Regras de negócio detalhadas que pertencem a `FEATURES.md`, `AI_PROMOTIONS.md` ou ao código. |
| Relacionar o termo canônico aos principais tipos, contratos ou documentos que o materializam. | Divergências pendentes de nomenclatura ou inventários exaustivos de implementação; esses conteúdos pertencem ao `ROADMAP.md` ou à documentação técnica responsável. |

Uma consequência semântica mínima pode fazer parte da definição. Por exemplo, dizer que o preço unitário preserva o valor usado no pedido explica o que **preço unitário** significa. Já a fórmula de cálculo, o arredondamento e a ordem das validações são regras e devem ficar na documentação da funcionalidade.

## Contexto do vocabulário

O contexto deste glossário é o sistema de gestão de pedidos de uma única lanchonete chamado Prensadão. O sistema cadastra clientes e produtos, registra e acompanha pedidos e produz sugestões de promoção.

O sistema não modela toda a operação comercial da lanchonete. Pagamento, estoque, autenticação, funcionários, entregadores e filiais, por exemplo, não fazem parte do modelo atual.

## Atores e limites

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Prensadão** | Nome do sistema de gestão de pedidos descrito neste repositório e contexto ao qual este glossário se aplica. | Não é produto do cardápio nem entidade persistida. Para o estabelecimento comercial, use **lanchonete**. | `Prensadao.sln`; `README.md` |
| **Lanchonete** | Estabelecimento comercial cuja operação de pedidos é apoiada pelo Prensadão. | Não é usuário, cliente, filial nem entidade persistida. O modelo atual não possui cadastro da lanchonete. | Contexto do projeto; não existe tipo correspondente |
| **Cliente** / `Customer` | Pessoa cadastrada à qual um pedido é associado e cujos dados identificam a referência e o contato do pedido. | Não é usuário autenticado, operador, consumidor RabbitMQ nem necessariamente quem manipulou a interface. O modelo não distingue comprador, pagador e destinatário. | `Customer`; DTOs de cliente |
| **Cozinha** | Etapa operacional de preparo do pedido. | Não é entidade, usuário autenticado, worker, espaço físico modelado nem fila RabbitMQ. Para o canal técnico, use **fila da cozinha**. | Status do pedido; notificações operacionais |

## Núcleo do Domínio

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Telefone** / `Phone` | Dado textual de contato pertencente ao cliente. | Não é identidade do usuário, conta de acesso ou garantia de que o contato recebeu uma mensagem. | `Customer.Phone`; DTOs de cliente |
| **Endereço do cliente** | Conjunto de dados de localização mantido no cadastro do cliente. | Não é entidade própria, conjunto de múltiplos endereços nem endereço preservado dentro de cada pedido. | `Customer`; DTOs de cliente |
| **CEP** / `Cep` | Código postal que compõe o endereço do cliente. | O modelo atual não preserva zero inicial. | `Customer.Cep`; DTOs de cliente |
| **Produto** / `Product` | Item cadastrado no cardápio, com nome, descrição, preço atual e estado de ativação. | Não é estoque, ingrediente, adicional, combo, categoria nem item já vendido. | `Product`; DTOs de produto |
| **Produto ativo** / `Enabled = true` | Produto disponível para compor novos pedidos. | Desativar não exclui o produto nem representa disponibilidade de estoque. | `Product.Enabled`; DTOs de produto |
| **Preço atual** / `Product.Price` | Preço vigente no cadastro do produto. | Não é o preço histórico preservado em um item de pedido. | `Product.Price`; DTOs de produto |

## Pedidos e Valores

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Pedido** / `Order` | Registro operacional associado a um cliente e composto por itens, data de criação, status, modalidade, total e observação. | Não é carrinho, orçamento, pagamento, comanda, entrega física nem mensagem RabbitMQ. | `Order`; DTOs de pedido |
| **Item de pedido** / `OrderItem` | Registro de um produto, sua quantidade e seu preço unitário dentro de um pedido. | Não é o próprio cadastro do produto. | `OrderItem`; DTOs de pedido |
| **Quantidade** / `Quantity` | Número de unidades do produto em um item de pedido. | Não representa estoque disponível nem quantidade de pedidos. | `OrderItem.Quantity`; `OrderItemRequestDto` |
| **Preço unitário** / `UnitPrice` | Preço preservado para uma unidade do produto dentro de um item de pedido. | Não é o preço atual do cadastro do produto. | `OrderItem.UnitPrice`; análises de vendas |
| **Total do item** | Valor monetário correspondente à quantidade e ao preço unitário de um item. | Não é o total completo do pedido. | `OrderItem`; análises de vendas |
| **Total do pedido** / `TotalAmount` | Valor monetário agregado dos itens do pedido. | Não comprova pagamento e não representa taxa de entrega, imposto, gorjeta ou desconto. | `Order.TotalAmount`; DTOs de pedido |
| **Entrega** / `IsDelivery` | Modalidade na qual o pedido segue para entrega depois do preparo. | Não é status, entidade logística, endereço de entrega nem cálculo de frete. | `Order.IsDelivery`; DTOs de pedido |
| **Retirada** | Modalidade na qual o pedido será retirado depois do preparo. | Não é entidade própria nem modalidade representada por um campo específico; corresponde à ausência de entrega no modelo atual. | `Order.IsDelivery`; notificações de pedido |
| **Observações do pedido** / `Notes` | Texto livre associado ao pedido. | Não é descrição do produto, log técnico nem mensagem de notificação. | `Order.Notes`; DTOs de pedido |
| **Data do pedido** / `CreatedAt` | Instante em que o pedido foi criado. | Não é data de pagamento, preparo, entrega ou finalização. | `Order.CreatedAt`; DTOs de pedido |
| **Criar pedido** | Registrar um novo pedido para um cliente cadastrado, com os produtos e quantidades solicitados. | Não identifica quem enviou a requisição, recebe pagamento nem reserva estoque. | `OrderService`; `FEATURES.md` |
| **Cancelar pedido** | Interromper o fluxo operacional mantendo o pedido no histórico como cancelado. | Não exclui o pedido, estorna pagamento nem reverte estoque. | `OrderService`; `FEATURES.md` |

## Status do Pedido

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Status do pedido** / `OrderStatusEnum` | Estado operacional atual do pedido. | Não é modalidade de entrega nem situação financeira. | `Order.Status`; `OrderStatusEnum` |
| **Criado** / `Criado = 0` | Pedido registrado e ainda não iniciado pela cozinha. | Não é apenas uma mensagem ainda não persistida. | `OrderStatusEnum` |
| **Em preparação** / `EmPreparacao = 1` | Pedido em preparo pela cozinha. | Não significa que o preparo foi concluído. | `OrderStatusEnum` |
| **Pronto** / `Pronto = 2` | Pedido cujo preparo foi concluído e que aguarda entrega ou retirada. | Não significa necessariamente finalizado. | `OrderStatusEnum` |
| **Saiu para entrega** / `SaiuParaEntrega = 3` | Pedido da modalidade entrega que foi despachado. | Não confirma o recebimento pelo cliente. | `OrderStatusEnum` |
| **Finalizado** / `Finalizado = 4` | Pedido cujo fluxo operacional foi encerrado. | Não comprova pagamento, entrega por terceiro nem baixa de estoque. | `OrderStatusEnum` |
| **Cancelado** / `Cancelado = 5` | Pedido interrompido e mantido no histórico. | Não é exclusão física nem estorno. | `OrderStatusEnum` |
| **Erro** / `Error = 99` | Valor excepcional usado no tratamento de status não reconhecido. | Não é etapa normal do pedido nem exceção .NET. | `OrderStatusEnum`; estratégias de notificação |
| **Avançar status** / `AdvanceStatus` | Levar o pedido à próxima etapa prevista no fluxo operacional. | Não é atribuição livre de qualquer estado. | `Order`; `OrderWorker` |
| **Atualizar status** / `SetStatus` | Atribuir ao pedido um estado informado pelo caso de uso de atualização. | Não é sinônimo de avançar para a próxima etapa. | `Order`; `OrderService` |

## Mensageria e Notificações

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Mensagem de pedido** / `OrderMessageDto` | Contrato assíncrono que identifica o pedido a ser encaminhado ao fluxo da cozinha. | Não é o agregado completo do pedido. | `OrderMessageDto`; `OrderWorker` |
| **Fila da cozinha** | Canal RabbitMQ pelo qual o Prensadão entrega mensagens de pedido ao processamento assíncrono da cozinha. | Não é a cozinha física nem o próprio pedido. | `RabbitMqConstants.Queues`; `OrderWorker` |
| **Mensagem de notificação** / `NotifyMessageDto` | Contrato com os dados necessários para produzir uma notificação sobre o pedido. | Não prova que uma comunicação externa foi enviada ou recebida. | `NotifyMessageDto`; `NotifyWorker` |
| **Notificação** | Conteúdo textual produzido de acordo com o status do pedido. | Não é SMS, WhatsApp, e-mail ou push efetivamente enviado. | Estratégias de notificação; `NotifyWorker` |
| **Consumidor** / `IConsumer` | Componente técnico que recebe mensagens de uma fila. | Não significa cliente da lanchonete. | `IConsumer`; workers |

## Vendas, IA e Promoções

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Análise de vendas** / `SalesAnalytics` | Leitura agregada dos itens de pedido dentro de um período. | Não é entidade persistida nem serviço analítico externo. | `ISalesAnalyticsRepository`; `AI_PROMOTIONS.md` |
| **Produto mais vendido** | Produto melhor classificado pela análise de vendas do Prensadão. | Não significa automaticamente produto de maior lucro nem maior número de pedidos distintos. | Modelos de análise; `AI_PROMOTIONS.md` |
| **Unidades vendidas** / `UnitsSold` | Soma das quantidades registradas para um produto no período analisado. | Não é número de pedidos distintos nem redução de estoque. | Modelos de análise |
| **Receita** / `Revenue` | Valor agregado dos itens usando seus preços unitários históricos. | Não representa lucro, dinheiro recebido nem soma dos preços atuais. | Modelos de análise |
| **Preço atual da análise** / `CurrentPrice` | Preço vigente do produto apresentado junto ao resultado da análise. | Não é o preço histórico usado no cálculo da receita. | Modelos de análise; `PromotionProductDto` |
| **Produto elegível para promoção** | Produto incluído entre os candidatos que podem ser selecionados para uma sugestão. | Não é qualquer produto do catálogo nem promoção já aprovada. | `PromotionGeneratorService`; `AI_PROMOTIONS.md` |
| **Promoção** | Sugestão para um produto, com nome comercial, desconto, preço promocional e melhor dia. | Não é entidade persistida, cupom, campanha ativada nem mudança permanente no preço do produto. | `PromotionProductDto`; `AI_PROMOTIONS.md` |
| **Sugestão da IA** / `PromotionAiSuggestionDto` | Resultado intermediário proposto pela IA para compor uma promoção. | Não é resposta final da API nem decisão aceita antes das validações do sistema. | `PromotionAiSuggestionDto`; `IPromotionSuggestionService` |
| **Nome da promoção** / `PromotionName` | Nome comercial proposto para a promoção. | Não identifica entidade persistida nem campanha ativa. | DTOs de promoção |
| **Preço promocional** / `PromotionalPrice` | Preço resultante da aplicação do desconto validado ao preço atual. | Não substitui o preço atual do produto nem altera pedidos automaticamente. | `PromotionProductDto`; `AI_PROMOTIONS.md` |
| **Melhor dia da semana** / `BestDayOfWeek` | Dia selecionado pelas análises do sistema para a sugestão de promoção. | Não é escolhido livremente pela IA nem garante resultado comercial. | `PromotionProductDto`; `AI_PROMOTIONS.md` |

Fórmulas, janelas de análise, limites de desconto, arredondamentos e validações da resposta da IA estão documentados em `AI_PROMOTIONS.md`.

## IA e LLM no contexto do projeto

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **IA** / **AI** | Integração externa usada para sugerir parte dos dados de uma promoção. | Não é ator do negócio, fonte de verdade nem responsável pelo ranking, melhor dia ou preço final. | Serviço de sugestão; `AI_PROMOTIONS.md` |
| **LLM** | *Large Language Model*: tecnologia usada pela integração de IA do Prensadão. | Não é sinônimo de toda a inteligência do sistema nem conceito do negócio da lanchonete. | Integração de IA; `AI_PROMOTIONS.md` |

## Conceitos que Não Estão Modelados

Não presuma a existência dos conceitos abaixo ao interpretar uma tarefa. Eles só devem entrar no vocabulário do Prensadão depois de serem explicitamente definidos e implementados:

- carrinho de compras ou checkout;
- pagamento, estorno, forma de pagamento ou situação financeira;
- distinção entre comprador, pagador e destinatário;
- estoque, ingrediente, adicional, combo ou categoria de produto;
- taxa, zona ou endereço específico de entrega no pedido;
- conta de usuário, autenticação, funcionário, operador identificado, entregador ou permissões;
- cupom ou campanha de promoção persistida;
- disparo real de SMS, WhatsApp, e-mail ou push;
- cadastro da lanchonete, unidade ou filial;
- exclusão lógica genérica para clientes ou pedidos.

Mencionar um desses conceitos em uma ideia ou requisito não o transforma automaticamente em parte do modelo. Seu significado e seus limites devem ser definidos antes da implementação.

## Critério de manutenção

Atualize este arquivo no mesmo trabalho quando uma mudança:

- criar, remover ou renomear um conceito do projeto;
- alterar o significado de um termo existente;
- introduzir um termo que possa ser confundido com outro ator, dado ou ação;
- modificar o significado de um campo de API ou mensagem;
- resolver ou introduzir uma ambiguidade entre camadas;
- transformar um conceito atualmente ausente em comportamento real.

Não inclua um termo apenas porque ele aparece no código. Antes de adicioná-lo, confirme que pelo menos uma destas perguntas tem resposta positiva:

1. O termo possui no Prensadão um significado mais específico ou diferente do uso comum?
2. Uma pessoa ou ferramenta de IA pode confundi-lo com outro ator, dado ou ação?
3. O termo é necessário para descrever o domínio sem ambiguidade?

Ao registrar uma regra, fórmula ou fluxo detalhado, coloque a descrição no documento responsável e mantenha aqui somente o significado dos termos envolvidos e a referência correspondente.
