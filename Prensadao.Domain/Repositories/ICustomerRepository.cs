using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<int> AddCustomerAsync(Customer customer);
        Task<Customer> GetByIdAsync(int id);
        Task<List<Customer>> GetCustomersAsync();
        Task UpdateAsync(Customer customer);
        Task<bool> PhoneIsExistsAsync(string phone);
        
    }
}
