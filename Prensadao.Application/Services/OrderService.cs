using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;
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
        private readonly IProductRepository _productRepository;

        public OrderService(IBus bus, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IProductRepository productRepository)
        {
            _bus = bus;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        public async Task<List<OrderReponseDto>> GetOrders()
        {
            var result = await _orderRepository.GetOrders();
            return OrderReponseDto.ToListDto(result);
        }

        public async Task<int> OrderCreate(OrderRequestDto dto)
        {
            if (dto is null)
                throw new ArgumentException("O pedido não pode ser nulo");

            if (!dto.OrderItems.Any() || dto.OrderItems.Any(x => x.ProductId <= 0))
                throw new ArgumentException("Pedido não pode ser feito sem items");

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado");

            List<int> productsIDs = dto.OrderItems.Select(x => x.ProductId).ToList();
            var verifyProductActive = await _productRepository.ExistsInactiveProduct(productsIDs);
            if (verifyProductActive)
                throw new ArgumentException("Pedido não pode ser feito com produtos inativos");

            var order = new Order(dto.Delivery, dto.Value, dto.Observation, dto.CustomerId);
            await _orderRepository.CreateOrder(order);

            foreach (var item in dto.OrderItems)
            {
                var ordemItem = new OrderItem(item.Quantity, item.Value, order.OrderId, item.ProductId);
                await _orderItemRepository.AddOrderItem(ordemItem);
            }

            await Message(order);

            return order.OrderId;
        }

        // Privates

        private async Task Message(Order order)
        {
            var messageDto = new OrderMessageDTO
            {
                OrderId = order.OrderId
            };

            await _bus.Publish(messageDto, RabbitMqConstants.Exchanges.OrderExchange);
        }
    }
}
