# Rules: Mensageria

## Regra
- Constantes de exchanges, filas e routing keys devem ficar centralizadas em `RabbitMqConstants` ou no local já estabelecido.
- Publicação de mensagens deve passar por `IBus`.
- Consumo deve passar por `IConsumer`.
- Workers ficam em `Prensadao.Infra/Messaging/Workers`.
- Workers devem passar o `CancellationToken` do host para o `IConsumer`.
- Contratos de mensagem devem ser serializados em JSON e manter compatibilidade.
- Ao alterar `OrderMessageDto` ou `NotifyMessageDto`, atualize produtores, consumidores, documentação e testes.
- Workers devem tratar cancelamento e falhas de forma previsível.
- Não bloqueie o host indefinidamente dentro de worker.
- Repositórios não devem publicar mensagens.
- Controllers não devem publicar mensagens diretamente.

## Exemplo Bom
Constantes centralizadas.

```csharp
public static class RabbitMqConstants
{
    public static class Exchanges
    {
        public const string OrderExchange = "prensado.order";
        public const string NotifyExchange = "prensado.notify";
    }
}
```

Publicação passando por abstração.

```csharp
private Task PublishOrderMessageAsync(Order order)
    => _bus.Publish(CreateOrderMessage(order), RabbitMqConstants.Exchanges.OrderExchange);
```

Consumo dentro de worker.

```csharp
await _consumer.Listen<OrderMessageDto>(
    RabbitMqConstants.Queues.OrderCozinhaQueue,
    async message =>
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var order = await orderRepository.GetByIdAsync(message.OrderId);

        if (order is null || order.Status != OrderStatusEnum.Criado)
            return;

        order.AdvanceStatus();
        await orderRepository.UpdateAsync(order);
    },
    stoppingToken);

await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
```

## Evite
Controller publicando diretamente no RabbitMQ.

```csharp
[HttpPost("Post")]
public async Task<IActionResult> PostAsync([FromBody] OrderRequestDto dto)
{
    var channel = _rabbitMqConnection.CreateModel();
    channel.BasicPublish("prensado.order", "", body);

    return Ok();
}
```

## Por quê?
Mensageria é detalhe de integração. Mantendo `IBus`, `IConsumer`, workers e constantes centralizadas, fica mais fácil trocar implementação, testar serviços e entender o fluxo assíncrono.
