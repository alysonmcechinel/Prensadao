using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<int> CreateOrder(Order order);
        Task Update(Order order);
        Task<List<Order>> GetOrders();
        Task<Order?> GetById(int id);
    }
}
