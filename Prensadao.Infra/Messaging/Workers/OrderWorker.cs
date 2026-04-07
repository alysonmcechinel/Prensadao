using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Messaging.Workers;

public class OrderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer _consumer;
    private readonly IBus _bus;

    public OrderWorker(IServiceProvider serviceProvider, IConsumer consumer, IBus bus)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _bus = bus;
    }

    // Exemplo de metodo com IDEMPOTÊNCIA
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => 
        _consumer.Listen<OrderMessageDto>(RabbitMqConstants.Queues.OrderCozinhaQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetService<IOrderRepository>();
            var order = await orderRepository.GetByIdAsync(message.OrderId);

            // 1. AQUI ESTÁ A IDEMPOTÊNCIA
            // Se o pedido já saiu do status 'Criado', ignoramos a mensagem.
            // Isso evita processar duas vezes se o RabbitMQ reenviar.
            if (order is null || order.Status != OrderStatusEnum.Criado)
                return;

            // 2. Execução Segura
            order.AdvanceStatus(); // Muda para 'Em Preparação'
            await orderRepository.UpdateAsync(order);

            // 3. Notifica apenas se a atualização ocorreu
            var notify = new NotifyMessageDto
            {
                OrderId = order.OrderId,
                ConsumerName = order.Customer.Name,
                Delivery = order.IsDelivery,
                OrderStatus = order.Status,
                Phone = order.Customer.Phone
            };
            await _bus.Publish(notify, RabbitMqConstants.Exchanges.NotifyExchange, "");

            Console.WriteLine($"Pedido #{order.OrderId} atualizado com sucesso.");
        });
}
