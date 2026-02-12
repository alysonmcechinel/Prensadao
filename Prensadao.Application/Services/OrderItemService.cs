using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    //TODO: implementar FluentValidation
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository;
        }

        public Task AddOrderItemAsync(OrderItem ordemItem) => _orderItemRepository.AddOrderItemAsync(ordemItem);

        public Task<List<OrderItem>> GetOrderItems() => _orderItemRepository.GetOrderItemsAsync();
    }
}
