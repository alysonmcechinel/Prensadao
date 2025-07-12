using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;
using Prensadao.Application.Publish;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBus _bus;
        private readonly IOrderRepository _orderRepository;

        public OrderService(IBus bus, IOrderRepository orderRepository)
        {
            _bus = bus;
            _orderRepository = orderRepository;
        }

        public Task<List<Order>> GetOrders() => _orderRepository.GetOrders();

        public async Task<int> OrderCreate(OrderDto dto)
        {
            var result = new OrderResultViewModel();

            if (dto is null)
                throw new ArgumentException("O pedido não pode ser nulo");

            if (!dto.Items.Any())
                throw new ArgumentException("Pedido não pode ser feito sem items");

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado");

            var order = new Order(dto.Delivery, dto.Value, dto.Observation, dto.CustomerId);

            await _orderRepository.CreateOrder(order);
            await _bus.Publish(dto, RabbitMqConstants.Exchanges.OrderExchange);

            return order.OrderId;
        }
    }
}
