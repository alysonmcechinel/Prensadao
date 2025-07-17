using Prensadao.Application.Models.Request;
using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetCustomers();
        Task<int> AddCustomer(CustomerDto dto);
    }
}
