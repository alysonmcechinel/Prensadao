# Glossário de Domínio do Prensadão

Este documento registra a Linguagem Ubíqua observada no código, nas migrations, nos contratos HTTP, nos testes e na documentação do Prensadão. Ele descreve o comportamento atual, inclusive divergências de nomenclatura que ainda existem entre as camadas.

Ao implementar ou refatorar uma regra de negócio, use os termos abaixo com estes significados. Não introduza conceitos comuns de e-commerce que não estejam modelados no sistema.

## Núcleo do Domínio

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Prensadão** | Sistema de gestão de pedidos de uma hamburgueria. | Não é o nome de um produto específico do cardápio. | `README.md`; solução `Prensadao.sln` |
| **Cliente** / `Customer` | Pessoa associada a pedidos. Possui nome, telefone e endereço, e pode ter vários pedidos. | Não significa usuário autenticado, operador, conta ou consumidor RabbitMQ. Esses conceitos não estão modelados como cliente. | `Domain/Entities/Customer.cs`; `CustomerService`; `CustomerController`; tabela `Customers` |
| **Telefone** / `Phone` | Identificador de contato textual do cliente. No cadastro, deve ter 10 ou 11 caracteres após `Trim` e não pode coincidir exatamente com um telefone já cadastrado. | A validação não comprova que todos os caracteres são dígitos e a unicidade é verificada pela aplicação na criação, não por constraint visível na migration. | `Customer.Phone`; `PhoneExtensions`; `CustomerService`; migration `fix-telefone` |
| **Endereço do cliente** | Conjunto de `Street`, `District`, `Number`, `City`, `ReferencePoint` e `Cep` armazenado no próprio cliente. | Não existe entidade `Address`, múltiplos endereços por cliente ou endereço próprio congelado no pedido. | `Customer`; `CustomerRequestDto`; `CustomerResponseDto`; `CustomerConfig` |
| **CEP** / `Cep` | Código postal do endereço do cliente, persistido como `int`. | Não é `string`; portanto, zero inicial não é preservado pelo modelo atual. | `Customer.Cep`; DTOs de cliente; tabela `Customers` |
| **Produto** / `Product` | Item do cardápio com nome, descrição, preço atual e indicador `Enabled`. Um produto pode aparecer em vários itens de pedido. | Não é estoque, ingrediente, combo, categoria nem item já vendido; esses conceitos não estão modelados. | `Domain/Entities/Product.cs`; `ProductService`; `ProductController`; tabela `Products` |
| **Produto ativo** / `Enabled = true` | Produto permitido na criação de um novo pedido. Produtos novos nascem ativos. | Desativar não exclui o produto nem seu histórico. `Enabled` não representa disponibilidade de estoque. | `Product.Enabled`; `Product.SetEnabled`; `ProductEnabledDto`; `ExistsInactiveByIdsAsync` |
| **Preço atual** / `Product.Price` | Preço vigente no cadastro do produto. | Não é necessariamente o preço praticado em pedidos antigos. Nos DTOs de produto, esse mesmo conceito ainda aparece como `Value`. | `Product.Price`; `ProductConfig`; `ProductResponseDto.Value`; `ProductRequestDto.Value` |
| **Nome de produto disponível** | Nome que ainda não foi encontrado por comparação exata no cadastro durante a criação do produto. | Não há normalização explícita de maiúsculas, espaços ou acentos, nem constraint única visível no banco. A atualização não repete essa verificação. | `ProductService.EnsureProductNameIsAvailableAsync`; `ProductRepository.ExistsByNameAsync` |

## Pedidos e Valores

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Pedido** / `Order` | Compra ligada obrigatoriamente a um cliente, com itens, data de criação, status, modalidade de entrega, valor total e observação. | Não é carrinho, orçamento, pagamento, comanda ou mensagem RabbitMQ. Esses conceitos não são equivalentes nem estão todos modelados. | `Domain/Entities/Order.cs`; `OrderService`; `OrderController`; tabela `Orders` |
| **Item de pedido** / `OrderItem` | Linha que liga um pedido a um produto e registra quantidade e preço unitário. | Não é o próprio produto. Alterações futuras no cadastro do produto não devem mudar `UnitPrice`. | `Domain/Entities/OrderItem.cs`; `OrderService`; tabela `OrderItems` |
| **Quantidade** / `Quantity` | Número de unidades de um produto em um item de pedido; deve ser maior que zero na criação do pedido. | Não representa estoque disponível nem quantidade de pedidos. | `OrderItem.Quantity`; `OrderItemRequestDto`; `OrderService.ValidateOrderItemsAsync` |
| **Preço unitário** / `UnitPrice` | Preço do produto capturado quando o item é criado. É a base histórica para o total do item e para `Revenue`. | Não é o preço atual do produto. A migration `fix-name` renomeou o antigo `OrderItems.Value` para `UnitPrice`. | `OrderItem.UnitPrice`; `OrderItem.GetTotalPrice`; migration `fix-name`; `SalesAnalyticsRepository` |
| **Total do item** | Resultado de `Quantity * UnitPrice`. | Não é persistido em uma propriedade própria e não representa o total do pedido. | `OrderItem.GetTotalPrice`; consultas de vendas |
| **Total do pedido** / `TotalAmount` | Soma de `quantidade * preço atual obtido na criação` para todos os itens, arredondada para duas casas com `AwayFromZero`. | Não inclui conceito modelado de taxa de entrega, imposto, gorjeta ou desconto. No contrato HTTP de resposta ainda aparece como `Value`. | `Order.TotalAmount`; `OrderService.CalculateTotalAmount`; `OrderResponseDto.Value`; migration `EntityNomenclature` |
| **Entrega** / `Delivery` / `IsDelivery` | Modalidade booleana do pedido. Se verdadeira, após `Pronto` o fluxo normal passa por `SaiuParaEntrega`; se falsa, segue para `Finalizado`. | Não é um status, entidade de logística, endereço de entrega ou cálculo de frete. | `Order.IsDelivery`; `OrderRequestDto.Delivery`; `OrderResponseDto.Delivery`; `NotifyMessageDto.Delivery` |
| **Retirada** | Interpretação operacional de um pedido com `IsDelivery = false`; a mensagem de pedido pronto informa que ele está pronto para retirada. | Não existe enum, campo ou entidade `Pickup`/`Retirada`. É uma consequência de `Delivery = false`. | `OrderReadyNotificationStrategy`; `Order.AdvanceStatus` |
| **Observação** / `Observation` / `Notes` | Texto livre do pedido. O contrato usa `Observation`; o domínio e o banco usam `Notes`. | Não é descrição de produto, log ou mensagem de notificação. | `Order.Notes`; `OrderRequestDto.Observation`; `OrderResponseDto.Observation`; migration `EntityNomenclature` |
| **Data do pedido** / `CreatedAt` / `DateOrder` | Instante de criação gravado em UTC no domínio; a resposta HTTP converte para `America/Sao_Paulo` e expõe como `DateOrder`. | O `DateTime` da resposta é local e `Unspecified`; não deve ser reinterpretado automaticamente como UTC. | `Order.CreatedAt`; `NodaTimeExtensions.NowUtc/LocalTime`; `OrderResponseDto.DateOrder` |
| **Criação do pedido** | Fluxo que valida cliente informado, itens, IDs, quantidades e produtos inativos; captura preços; persiste pedido e itens em `TransactionScope`; depois publica mensagem para a cozinha. | Não valida estoque ou pagamento. A existência real do cliente é delegada à persistência/FK, não consultada explicitamente pelo serviço. | `OrderService.OrderCreateAsync`; `OrderServiceTest`; `OrderMessageDto` |
| **Cancelamento do pedido** | Mudança do status para `Cancelado`, permitida pelo serviço somente quando o pedido está `Criado` ou `EmPreparacao`, seguida de notificação. | Não exclui o pedido, não estorna pagamento e não reverte estoque. O endpoint que cancela chama-se atualmente `Enabled`, nome que não expressa a ação. | `OrderService.EnabledAsync`; `OrderController.EnabledAsync`; `OrderCanceledNotificationStrategy` |

## Status do Pedido

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Status do pedido** / `OrderStatusEnum` | Estado operacional persistido como inteiro e apresentado por descrição textual nos DTOs de resposta. | Não é o mesmo que modalidade de entrega. `UpdateStatusAsync` atribui diretamente o status recebido; `AdvanceStatus` segue o fluxo sequencial. | `Order.Status`; `OrderStatusEnum`; `UpdateStatusDto`; `OrderResponseDto.OrderStatus` |
| **Criado** / `Criado = 0` | Estado inicial de todo pedido novo, antes do preparo. | Não significa apenas que uma mensagem foi publicada; o pedido já está persistido. | Construtor de `Order`; enum; `OrderWorker` |
| **Em preparação** / `EmPreparacao = 1` | Pedido em preparo. É o avanço normal a partir de `Criado`. | Não significa que o pedido está pronto. | `Order.AdvanceStatus`; `OrderPreparingNotificationStrategy` |
| **Pronto** / `Pronto = 2` | Preparo concluído; aguarda entrega ou retirada conforme a modalidade. | Não significa necessariamente finalizado. | `Order.AdvanceStatus`; `OrderReadyNotificationStrategy` |
| **Saiu para entrega** / `SaiuParaEntrega = 3` | Pedido com entrega que foi despachado. | Não se aplica ao fluxo normal sem entrega. | `Order.AdvanceStatus`; `OrderOutForDeliveryNotificationStrategy` |
| **Finalizado** / `Finalizado = 4` | Fluxo operacional concluído. | Não comprova pagamento, entrega por terceiro ou baixa de estoque, pois esses conceitos não são modelados. | `Order.AdvanceStatus`; `OrderFinishedNotificationStrategy` |
| **Cancelado** / `Cancelado = 5` | Pedido interrompido e mantido no histórico. | Não é exclusão física. As análises de venda atuais não filtram pedidos cancelados. | `OrderService.EnabledAsync`; `OrderCanceledNotificationStrategy`; enum |
| **Erro** / `Error = 99` | Valor excepcional do enum associado à estratégia de mensagem para status desconhecido. | Não faz parte do fluxo normal de `AdvanceStatus` e não representa uma exceção .NET específica. | `OrderStatusEnum`; `UnknownOrderStatusNotificationStrategy` |
| **Avançar status** / `AdvanceStatus` | Transição sequencial `Criado -> EmPreparacao -> Pronto -> SaiuParaEntrega -> Finalizado`, pulando `SaiuParaEntrega` quando não há entrega. | Não é equivalente a `SetStatus`, que aceita atribuição direta. `AdvanceStatus` rejeita estados sem próxima transição. | `Order.AdvanceStatus`; `OrderWorker` |
| **Atualizar status** / `SetStatus` | Atribuição do status recebido pelo endpoint, desde que seja diferente do estado atual. | Não garante a sequência de `AdvanceStatus`; atualmente pode saltar etapas se o contrato enviar outro valor do enum. | `Order.SetStatus`; `OrderService.UpdateStatusAsync`; `PUT api/Order/UpdateStatus` |

Fluxo sequencial da entidade:

```text
Criado -> EmPreparacao -> Pronto -> SaiuParaEntrega -> Finalizado
                                 \-> Finalizado (Delivery = false)
```

## Mensageria e Notificações

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Mensagem de pedido** / `OrderMessageDto` | Contrato assíncrono contendo apenas `OrderId`, publicado após a criação para iniciar o processamento da cozinha. | Não carrega o agregado completo do pedido. | `OrderMessageDTO.cs`; `OrderService`; `OrderWorker`; exchange `prensado.order` |
| **Mensagem de notificação** / `NotifyMessageDto` | Contrato com pedido, status, modalidade, nome e telefone usados pelo fluxo de notificação. | Não prova que uma comunicação externa foi enviada. | `NotifyMessageDTO.cs`; `OrderService`; `OrderWorker`; `NotifyWorker` |
| **Notificação** | Texto escolhido por uma estratégia conforme o status do pedido. No estado atual, o worker escreve esse texto no console. | Não é SMS, WhatsApp, e-mail ou push; nenhum provedor externo está implementado. | `IOrderStatusNotificationStrategy`; classes em `Services/Notifications`; `NotifyWorker` |
| **Cozinha** | Destino lógico da fila de pedidos que avança um pedido de `Criado` para `EmPreparacao`. | Não é entidade, usuário ou bounded context separado no modelo atual. | fila `prensado.orders.cozinha`; `OrderWorker`; `RabbitMqConstants` |
| **Publicador** / `IMessagePublisher` | Abstração da aplicação para publicar mensagens em exchanges RabbitMQ. | Não é repositório nem regra de negócio. | `IMessagePublisher`; `RabbitMqMessagePublisher`; `OrderService`; `OrderWorker` |
| **Consumidor** / `IConsumer` | Abstração técnica que escuta uma fila RabbitMQ e entrega mensagens ao callback. | Não significa cliente da hamburgueria. | `IConsumer`; `Consumer`; `OrderWorker`; `NotifyWorker` |
| **Worker de pedido** / `OrderWorker` | Consumidor idempotente da fila da cozinha: somente pedidos ainda em `Criado` são avançados e notificados. | Não processa continuamente todas as etapas do pedido em uma única mensagem. | `Infra/Messaging/Workers/OrderWorker.cs` |
| **Worker de notificação** / `NotifyWorker` | Consumidor que escolhe a estratégia pelo status e imprime a mensagem resultante. | Não envia efetivamente a mensagem ao telefone presente no DTO. | `Infra/Messaging/Workers/NotifyWorker.cs` |
| **Exchange** | Destino RabbitMQ usado na publicação (`prensado.order` ou `prensado.notify`). | Não é fila; exchanges são ligadas às filas durante o startup. | `RabbitMqConstants.Exchanges`; `RabbitMqStartup`; `RabbitMqMessagePublisher` |
| **Fila** / `Queue` | Canal RabbitMQ consumido pelos workers (`prensado.orders.cozinha` e `prensado.orders.notify`). | Não é a mesma coisa que exchange ou lista persistida de pedidos. | `RabbitMqConstants.Queues`; `RabbitMqStartup`; workers |
| **Routing key** | Chave `prensado.order.created` definida para roteamento de criação de pedido. | A presença da constante não significa que todo publish a utiliza; chamadas atuais podem publicar com chave vazia. | `RabbitMqConstants.RoutingKeys`; infraestrutura RabbitMQ |

## Vendas, IA e Promoções

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **Análise de vendas** / `SalesAnalytics` | Consultas agregadas sobre `OrderItems` para classificar produtos e dias da semana. | Não é um agregado persistido nem serviço analítico externo. As consultas atuais não filtram status do pedido nem `Product.Enabled`. | `ISalesAnalyticsRepository`; `SalesAnalyticsRepository`; read models `TopProductSalesModels` |
| **Produto mais vendido** | Produto classificado primeiro por soma de unidades e, em empate, por receita, dentro do intervalo consultado. | Não é classificado apenas por receita, margem ou quantidade de pedidos. | `GetTopProductsSoldAsync`; `TopProductSalesModels` |
| **Top N** / `TopN` | Limite máximo de produtos mais vendidos enviados à geração da promoção; deve ser maior que zero e assume 10 no request. | Não é mínimo de vendas, quantidade de pedidos nem número de promoções geradas. | `GeneratePromotionRequest`; `PromotionGeneratorService`; `Take(topN)` |
| **Unidades vendidas** / `UnitsSold` | Soma de `OrderItem.Quantity` por produto no período. | Não é número de pedidos distintos nem estoque reduzido. | `SalesAnalyticsRepository`; `TopProductSalesModels`; `TopProductSalesDto` |
| **Receita** / `Revenue` | Soma de `Quantity * UnitPrice` dos itens agrupados para o produto. | Não usa `Product.Price` atual e não representa lucro. Atualmente inclui itens de pedidos independentemente do status. | `SalesAnalyticsRepository`; `TopProductSalesModels`; `TopProductSalesDto` |
| **Preço atual da análise** / `CurrentPrice` | `Product.Price` atual projetado junto ao resultado agregado. | Não é o preço histórico usado para calcular `Revenue`. | `TopProductSalesModels.CurrentPrice`; `TopProductSalesDto`; `PromotionProductDto` |
| **Produto elegível para promoção** | Produto presente na lista `topProducts` enviada à IA; a resposta só pode selecionar um ID dessa lista. | Não significa necessariamente produto ativo: a consulta atual de vendas não filtra `Enabled`. Também não é qualquer produto do catálogo. | `PromotionGeneratorService`; `AzureOpenAiPromotionService.ValidationPromotion` |
| **Promoção** | Sugestão calculada para um único produto, com nome comercial, desconto, preço promocional e melhor dia. | Não é entidade persistida, cupom, campanha ativada ou mudança permanente no preço do produto. | endpoint `POST api/Promotions/Generate`; `PromotionProductDto` |
| **Promoção semanal** / `GenerateWeeklyPromotionAsync` | Nome do caso de uso que gera uma sugestão e inclui um melhor dia da semana. | O nome não significa que todos os dados vêm dos últimos sete dias: o código atual consulta 360 dias para produtos e 84 dias para o melhor dia. | `IPromotionGenerator`; `PromotionGeneratorService` |
| **Sugestão da IA** / `PromotionAiSuggestionDto` | Saída intermediária da IA contendo `ProductId` textual, `PromotionName` e `DiscountPercent`. | Não é a resposta final da API e não é confiável antes das validações. | `PromotionAiSuggestionDto`; `IPromotionSuggestionService`; `AzureOpenAiPromotionService` |
| **Nome da promoção** / `PromotionName` | Nome comercial obrigatório proposto pela IA. | Não identifica uma entidade persistida e não precisa ser único no estado atual. | `PromotionAiSuggestionDto`; `PromotionProductDto`; validação da IA |
| **Desconto mínimo** / `MinDiscountPercent` | Limite inferior permitido para a porcentagem sugerida; padrão 5 e pode ser zero. | Não é valor monetário. Não pode ser maior que o máximo. | `GeneratePromotionRequest`; `PromotionGeneratorService`; prompt da IA |
| **Desconto máximo** / `MaxDiscountPercent` | Limite superior permitido; padrão 25 e deve ser maior que zero. | Não é preço final nem valor monetário. | `GeneratePromotionRequest`; `PromotionGeneratorService`; prompt da IA |
| **Desconto seguro** / `safeDiscount` | Percentual da sugestão limitado por `Math.Clamp` aos limites do request antes do cálculo final. | Não substitui a validação da integração, que já rejeita valor fora dos limites; é defesa adicional no gerador. | `PromotionGeneratorService` |
| **Preço promocional** / `PromotionalPrice` | `CurrentPrice * (1 - desconto/100)`, arredondado para duas casas com `AwayFromZero`. | Não é persistido em `Product.Price` e não altera pedidos. | `PromotionGeneratorService`; `PromotionProductDto` |
| **Melhor dia da semana** / `BestDayOfWeek` | Dia com maior receita média por data distinta no intervalo de 84 dias; unidades vendidas desempata. Sem dados, retorna `Friday`. | Não é o dia de menor movimento, não é escolhido livremente pela IA e atualmente é retornado em inglês pelo enum .NET. | `GetBestPromotionDayOfWeekAsync`; `PromotionGeneratorService`; `PromotionProductDto` |
| **Resposta válida da IA** | JSON aderente ao schema, com nome não vazio, ID numérico pertencente aos produtos elegíveis e desconto dentro dos limites. | Texto plausível, Markdown ou produto inventado continuam inválidos. | `AzureOpenAiPromotionService.ValidationPromotion`; `AiResponseValidationException` |
| **Schema de promoção** | JSON Schema que restringe a resposta da IA aos campos `productId`, `promotionName` e `discountPercent`, sem propriedades adicionais. | Não é schema do banco nem contrato final completo da API. | `AzureOpenAiPromotionService.DefinitionUserSystem` |
| **Falha de validação da IA** / `AiResponseValidationException` | Exceção usada quando a saída do provider está vazia, não é JSON ou viola o contrato semântico. | Não é falha de conexão com Azure e não equivale a qualquer `InvalidOperationException` do gerador. | `Application/Exceptions/AiResponseValidationException.cs`; `AzureOpenAiPromotionService`; `RequestContextMiddleware` |

## Siglas e Termos de Contrato

| Termo | Definição no Prensadão | O que NÃO significa / Cuidados | Onde é usado |
| --- | --- | --- | --- |
| **ID** | Identificador inteiro das entidades (`CustomerId`, `ProductId`, `OrderId`, `OrderItemId`). | Valor positivo não garante existência. Na sugestão bruta da IA, `ProductId` chega como `string` e deve ser convertido. | Entidades, DTOs, repositories e controllers |
| **DTO** | Objeto de transporte entre API, aplicação, mensageria ou integração com IA. | Não é entidade persistida nem deve ser tratado automaticamente como agregado de domínio. | `Prensadao.Application/DTOs/` |
| **API** | Interface HTTP ASP.NET Core exposta pelos controllers de cliente, produto, pedido e promoções. | Não inclui diretamente os workers RabbitMQ. | `Prensadao/Controllers/`; `Program.cs` |
| **IA** / **AI** | Capacidade externa usada exclusivamente para sugerir os três campos da promoção. O código mistura português (`IA`) e inglês (`Ai`, `AI`). | Não calcula ranking, melhor dia nem preço final; essas decisões permanecem no código. | `AzureOpenAiPromotionService`; `AiResponseValidationException`; opções `AzureOpenAi` |
| **LLM** | Nome técnico da pasta de infraestrutura que contém a integração com Azure OpenAI. | Não existe como entidade ou conceito de negócio. | `Prensadao.Infra/llm/` |
| **UTC** | Referência temporal usada para persistir e consultar `CreatedAt`. | Não é o formato apresentado ao cliente da API, que é convertido para São Paulo. | `NodaTimeExtensions`; `OrderService`; `SalesAnalyticsRepository` |
| **JSON** | Formato dos contratos HTTP, mensagens RabbitMQ e resposta estruturada da IA. | Mesmo JSON sintaticamente válido pode ser semanticamente inválido para uma promoção. | Controllers; mensageria; `AzureOpenAiPromotionService` |
| **Correlation ID** / `X-Correlation-Id` | Identificador técnico de rastreamento criado ou propagado pelo middleware de erros. | Não é ID de pedido ou cliente. O middleware está atualmente comentado no pipeline. | `RequestContextMiddleware`; `Program.cs` |

## Ambiguidades e Divergências Atuais

Esta seção é normativa para interpretação, mas não declara que as divergências sejam desejáveis. Ao refatorá-las, preserve os contratos públicos ou trate a mudança como quebra de contrato.

| Termos em conflito | Interpretação correta hoje | Onde ocorre |
| --- | --- | --- |
| `Price` × `Value` | No domínio e no banco, produto usa `Price`; nos DTOs de produto, o mesmo valor ainda se chama `Value`. | `Product`; `ProductRequestDto`; `ProductResponseDto`; migration `EntityNomenclature` |
| `TotalAmount` × `Value` | No domínio e banco, o total do pedido é `TotalAmount`; no `OrderResponseDto`, ele é exposto como `Value`. | `Order`; `OrderResponseDto`; migration `EntityNomenclature` |
| `UnitPrice` × `ProductValue` | `UnitPrice` é o preço histórico. Porém, `OrderItemResponseDto.ProductValue` é preenchido atualmente com `Product.Price`, isto é, o preço atual. Não assuma que esse campo HTTP preserva o valor da venda. | `OrderItem`; `OrderResponseDto.ToListItens` |
| `Notes` × `Observation` | `Notes` é o nome do domínio/banco; `Observation` é o nome do request e response HTTP. | `Order`; DTOs de pedido; migration `EntityNomenclature` |
| `CreatedAt` × `DateOrder` | `CreatedAt` é UTC no domínio; `DateOrder` é a apresentação local no DTO de resposta. | `Order`; `OrderResponseDto`; `NodaTimeExtensions` |
| `Status` × `OrderStatus` | `Status` é o campo do domínio; `OrderStatus` é usado nos DTOs. Na resposta é texto descritivo; no request é enum. | `Order`; `UpdateStatusDto`; `OrderResponseDto` |
| `Delivery` × `IsDelivery` | `IsDelivery` é o nome booleano no domínio; `Delivery` permanece nos contratos. `false` implica retirada apenas pelo comportamento, sem campo próprio. | `Order`; DTOs; migration `EntityNomenclature` |
| `Customer` × `Consumer` | Cliente é `Customer`. `Consumer` normalmente significa consumidor RabbitMQ. O campo legado `NotifyMessageDto.ConsumerName` contém, na prática, o nome do cliente. | `Customer`; `IConsumer`; `NotifyMessageDto` |
| `Enabled` de produto × `Enabled` de pedido | Em produto, `Enabled` ativa/desativa o catálogo. Em `OrderController`, o endpoint `Enabled` cancela o pedido e não altera flag alguma. | `ProductController.Enabled`; `OrderController.Enabled`; services correspondentes |
| `AdvanceStatus` × `SetStatus` | O primeiro aplica a máquina sequencial; o segundo atribui diretamente o enum enviado pela API. | `Order`; `OrderWorker`; `OrderService.UpdateStatusAsync` |
| “Semanal” × janelas de análise | O caso de uso se chama semanal, mas usa 360 dias para ranking e 84 dias para melhor dia. | `PromotionGeneratorService.GenerateWeeklyPromotionAsync` |
| “Elegível” × produto ativo | Elegível significa estar na lista entregue à IA. A consulta que forma essa lista não filtra `Enabled`, então um produto inativo pode aparecer. | `SalesAnalyticsRepository`; `AzureOpenAiPromotionService` |
| “Vendas” × status do pedido | As métricas agregam itens sem filtrar `Order.Status`; pedidos cancelados também podem participar. | `SalesAnalyticsRepository` |
| Notificação × envio | O contrato contém telefone e nome, mas o worker atual apenas escreve no console. | `NotifyMessageDto`; `NotifyWorker` |
| `Error` × status desconhecido | `Error = 99` é um membro real do enum e também identifica a estratégia fallback, mas um valor desconhecido não é automaticamente convertido para 99. | `OrderStatusEnum`; `OrderStatusNotificationStrategyFactory` |

## Conceitos que Não Estão Modelados

Não presuma a existência dos conceitos abaixo ao interpretar uma tarefa. Eles só devem entrar na Linguagem Ubíqua depois de serem explicitamente definidos e implementados:

- carrinho de compras ou checkout;
- pagamento, estorno, forma de pagamento ou situação financeira;
- estoque, ingrediente, adicional, combo ou categoria de produto;
- taxa, zona ou endereço específico de entrega no pedido;
- usuário autenticado, funcionário, entregador ou permissões;
- cupom ou campanha de promoção persistida;
- disparo real de SMS, WhatsApp, e-mail ou push;
- unidade/filial da hamburgueria;
- exclusão lógica genérica para clientes ou pedidos.

## Regra de Evolução

Atualize este arquivo no mesmo trabalho quando uma mudança:

- criar, remover ou renomear um conceito de negócio;
- alterar uma invariável, transição de status ou fórmula;
- modificar o significado de um campo de API ou mensagem;
- resolver ou introduzir uma ambiguidade entre camadas;
- transformar um conceito atualmente ausente em comportamento real.

Termos puramente técnicos e inequívocos não precisam ser adicionados, a menos que afetem diretamente a interpretação de uma regra do Prensadão.
