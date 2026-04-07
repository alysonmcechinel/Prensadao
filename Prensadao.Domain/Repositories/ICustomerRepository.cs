using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<int> AddAsync(Customer customer);
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer?> GetByIdWithDetailsAsync(int id);
        Task<List<Customer>> GetAllWithDetailsAsync();
        Task UpdateAsync(Customer customer);
        Task<bool> ExistsByPhoneAsync(string phone);
    }
}
