using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddOrderItem(OrderItem ordemItem);
        Task<List<OrderItem>> GetOrderItems();
    }
}
