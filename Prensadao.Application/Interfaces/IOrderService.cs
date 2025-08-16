using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task Enabled(int id);
        Task<List<OrderReponseDto>> GetOrders();
        Task<int> OrderCreate(OrderRequestDto dto);
        Task<OrderReponseDto> UpdateStatus(UpdateStatusDTO dto);
    }
}
