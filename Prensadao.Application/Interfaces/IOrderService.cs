using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface IOrderService
    {
        Task EnabledAsync(int id);
        Task<List<OrderResponseDto>> GetOrdersAsync();
        Task<OrderResponseDto> GetByIdAsync(int id);
        Task<int> OrderCreateAsync(OrderRequestDto dto);
        Task<OrderResponseDto> UpdateStatusAsync(UpdateStatusDto dto);
    }
}
