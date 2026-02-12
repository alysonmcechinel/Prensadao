using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<int> AddCustomerAsync(CustomerRequestDto dto);
        Task<CustomerResponseDto> GetByIdAsync(int id);
        Task<List<CustomerResponseDto>> GetCustomersAsync();
        Task UpdateAsync(CustomerRequestDto dto);
    }
}
