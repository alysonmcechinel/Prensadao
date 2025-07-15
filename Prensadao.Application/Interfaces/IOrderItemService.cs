using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderItemService
    {
        Task AddOrderItem(OrderItem ordemItem);
        Task<List<OrderItem>> GetOrderItems();
    }
}
