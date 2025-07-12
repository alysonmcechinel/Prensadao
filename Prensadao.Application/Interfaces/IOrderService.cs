using Prensadao.Application.Models;
using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrders();
        Task<int> OrderCreate(OrderDto dto);
    }
}
