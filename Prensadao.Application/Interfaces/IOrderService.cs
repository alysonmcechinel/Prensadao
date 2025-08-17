using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task Enabled(int id);
        Task<List<OrderResponseDto>> GetOrders();
        Task<OrderResponseDto> GetById(int id);
        Task<int> OrderCreate(OrderRequestDto dto);
        Task<OrderResponseDto> UpdateStatus(UpdateStatusDto dto);
    }
}
