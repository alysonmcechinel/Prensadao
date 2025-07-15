using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;
using Prensadao.Application.Publish;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBus _bus;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderService(IBus bus, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
        {
            _bus = bus;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        public Task<List<Order>> GetOrders() => _orderRepository.GetOrders();

        public async Task<int> OrderCreate(OrderDto dto)
        {
            if (dto is null)
                throw new ArgumentException("O pedido não pode ser nulo");

            if (!dto.Items.Any() || dto.Items.Any(x => x.ProductId <= 0))
                throw new ArgumentException("Pedido não pode ser feito sem items");

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado");

            var order = new Order(dto.Delivery, dto.Value, dto.Observation, dto.CustomerId);
            await _orderRepository.CreateOrder(order);

            foreach (var item in dto.Items)
            {
                var ordemItem = new OrderItem(item.Quantity, item.Value, order.OrderId, item.ProductId);
                await _orderItemRepository.AddOrderItem(ordemItem);
            }
                        
            await _bus.Publish(dto, RabbitMqConstants.Exchanges.OrderExchange);

            return order.OrderId;
        }
    }
}
