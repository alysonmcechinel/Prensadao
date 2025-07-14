using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetCustomers();
        Task<int> AddCustomer(Customer customer);
    }
}
