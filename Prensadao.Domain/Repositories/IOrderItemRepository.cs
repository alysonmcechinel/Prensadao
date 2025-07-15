using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderItemRepository
    {
        Task AddOrderItem(OrderItem ordemItem);
        Task<List<OrderItem>> GetOrderItems();
    }
}
