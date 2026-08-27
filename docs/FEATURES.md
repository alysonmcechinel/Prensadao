# Funcionalidades

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

## Pedidos
- Criação de pedidos com cliente e itens.
- Cálculo do total com base nos produtos.
- Persistência do pedido e itens dentro de fluxo transacional.
- Consulta por id e listagem.
- Atualização de status.
- Cancelamento via status `Cancelado`, respeitando regras de status permitido.
- Publicação de mensagens para cozinha e notificação.

## Mensageria e Workers
- RabbitMQ usa exchanges e filas centralizadas em `RabbitMqConstants`.
- `OrderWorker` consome pedidos da fila da cozinha e publica notificação.
- `NotifyWorker` consome mensagens de notificação.
- A inicialização de exchanges, filas e binds ocorre via `RabbitMqStartup`.

## Hangfire
- Hangfire é registrado na infraestrutura usando PostgreSQL.
- O dashboard é habilitado no pipeline HTTP.

## Promoções com IA
- Endpoint `POST api/Promotions/Generate`.
- Seleciona produtos mais vendidos via `ISalesAnalyticsRepository`.
- Usa Azure OpenAI via `IPromotionSuggestionService`.
- Valida resposta da IA antes de calcular o preço promocional.
- Detalhes em `AI_PROMOTIONS.md`.

## Pendências Conhecidas
- Tratamento global de erros existe em `RequestContextMiddleware`, mas está comentado em `Program.cs`.
- A feature de promoções com IA ainda precisa de testes dedicados.
