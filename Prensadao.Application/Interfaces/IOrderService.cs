using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderReponseDto>> GetOrders();
        Task<int> OrderCreate(OrderRequestDto dto);
    }
}
