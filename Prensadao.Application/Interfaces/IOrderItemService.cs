using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderItemService
    {
        Task AddOrderItemAsync(OrderItem ordemItem);
        Task<List<OrderItem>> GetOrderItems();
    }
}
