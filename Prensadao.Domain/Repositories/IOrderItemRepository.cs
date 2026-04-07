using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddAsync(OrderItem orderItem);
        Task<List<OrderItem>> GetAllAsync();
    }
}
