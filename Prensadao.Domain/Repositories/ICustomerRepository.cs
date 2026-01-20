using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<int> AddCustomerAsync(Customer customer);
        Task<Customer> GetById(int id);
        Task<List<Customer>> GetCustomers();
        Task Update(Customer customer);
        Task<bool> PhoneIsExists(string phone);
        
    }
}
