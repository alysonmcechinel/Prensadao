using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<int> AddCustomer(CustomerRequestDto dto);
        Task<CustomerResponseDto> GetById(int id);
        Task<List<CustomerResponseDto>> GetCustomers();
        Task Update(CustomerRequestDto dto);
    }
}
