using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;
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
