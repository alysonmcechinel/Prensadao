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

        public Task AddOrderItemAsync(OrderItem orderItem)
        {
            ArgumentNullException.ThrowIfNull(orderItem);
            return _orderItemRepository.AddAsync(orderItem);
        }

        public Task<List<OrderItem>> GetOrderItemsAsync() => _orderItemRepository.GetAllAsync();
    }
}
