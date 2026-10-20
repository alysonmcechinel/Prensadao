# Funcionalidades

Este documento registra apenas comportamentos já implementados. Trabalho futuro e débitos conhecidos ficam em `ROADMAP.md`.

## Clientes

- Cadastro de clientes.
- Consulta por id.
- Listagem.
- Atualização de dados cadastrais.
- Validação de id e telefone duplicado nos serviços de aplicação.

## Produtos

- Cadastro de produtos.
- Consulta por id.
- Listagem.
- Atualização de dados.
- Ativação/desativação por flag `Enabled`.
- Validações de nome obrigatório, preço positivo, id válido, existência e nome duplicado.
- Requests e responses de produto expõem o preço pelo campo canônico `Price`.

## Pedidos

- Criação de pedidos com cliente e itens.
- Cálculo do total com base nos produtos.
- Persistência do pedido e itens dentro de fluxo transacional.
- Consulta por id e listagem.
- Atualização de status por `PUT api/Order/Status`, com `OrderId` e `Status` no corpo, aceitando somente a próxima etapa do fluxo operacional; repetir o status atual não persiste nem publica um novo evento.
- Pedidos de entrega seguem `Criado` -> `EmPreparacao` -> `Pronto` -> `SaiuParaEntrega` -> `Finalizado`; pedidos de retirada seguem de `Pronto` diretamente para `Finalizado`.
- Cancelamento por `POST api/Order/{id}/Cancel`, permitido somente nos estados `Criado` e `EmPreparacao`.
- `Cancelado` não pode ser atribuído pelo endpoint comum de atualização de status.
- Publicação de mensagens para a cozinha e de eventos de alteração de status.
- Responses de pedido expõem o valor total pelo campo canônico `TotalAmount`.
- Itens de pedido expõem o preço histórico cobrado pelo campo `UnitPrice`.
- Requests e responses de pedido usam `Notes` para as observações do pedido.
- Responses de pedido expõem `CreatedAt` em `America/Sao_Paulo` com offset explícito, enquanto a persistência permanece em UTC.
- O contrato HTTP de atualização recebe no corpo `OrderId` e o número de `Status` definido em `OrderStatusEnum`; as responses retornam a descrição legível do estado.
- Domínio, contratos HTTP e mensagens usam `IsDelivery`; no JSON HTTP, `isDelivery` informa obrigatoriamente se o pedido é entrega (`true`) ou retirada (`false`).

## Mensageria e Workers

- RabbitMQ usa exchanges e filas centralizadas em `RabbitMqConstants`.
- `OrderWorker` consome pedidos da fila da cozinha e publica um evento de alteração de status.
- `OrderStatusMessageWorker` consome eventos `OrderStatusChangedMessageDto`, gera o texto correspondente por estratégia e o registra no log da aplicação.
- `Criado` não produz mensagem de status. Valores de status inválidos são registrados como falha técnica e não são convertidos em estados do pedido.
- O sistema ainda não envia SMS, WhatsApp, e-mail ou push.
- A inicialização de exchanges, filas e binds ocorre via `RabbitMqStartup`.

## Hangfire

- Hangfire é registrado na infraestrutura usando PostgreSQL.
- O dashboard é habilitado no pipeline HTTP.

## Promoções com IA

- Endpoint `POST api/Promotions/Generate`.
- Seleciona produtos mais vendidos via `ISalesAnalyticsRepository`.
- Calcula métricas de vendas e receita somente com pedidos finalizados.
- Considera apenas produtos ativos entre os candidatos enviados à IA.
- Usa Azure OpenAI via `IPromotionSuggestionService`.
- Valida resposta da IA antes de calcular o preço promocional.
- Detalhes em `AI_PROMOTIONS.md`.
