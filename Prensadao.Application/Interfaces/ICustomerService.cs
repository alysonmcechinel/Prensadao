using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Domain.Entities;

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
