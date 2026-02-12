using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddOrderItemAsync(OrderItem ordemItem);
        Task<List<OrderItem>> GetOrderItemsAsync();
    }
}
