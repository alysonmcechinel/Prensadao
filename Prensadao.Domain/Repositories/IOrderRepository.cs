using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<int> CreateOrder(Order order);
        Task<List<Order>> GetOrders();
    }
}
