using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<int> AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task<Order?> GetByIdWithDetailsAsync(int id);
        Task<List<Order>> GetAllWithDetailsAsync();
        Task<Order?> GetByIdAsync(int id);
    }
}
