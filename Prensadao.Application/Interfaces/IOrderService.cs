using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;
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
        Task<List<OrderReponseDto>> GetOrders();
        Task<int> OrderCreate(OrderRequestDto dto);
    }
}
