using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public async Task AddOrderItem(OrderItem ordemItem) => await _orderItemRepository.AddOrderItem(ordemItem);

        public async Task<List<OrderItem>> GetOrderItems() => await _orderItemRepository.GetOrderItems();
    }
}
