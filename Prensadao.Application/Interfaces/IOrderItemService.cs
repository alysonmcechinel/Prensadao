using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderItemService
    {
        Task AddOrderItemAsync(OrderItem orderItem);
        Task<List<OrderItem>> GetOrderItemsAsync();
    }
}
