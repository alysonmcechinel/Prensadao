using Prensadao.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResultViewModel> OrderCreate(OrderDto dto);
    }
}
