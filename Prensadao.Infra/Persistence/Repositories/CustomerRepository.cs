using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly PrensadaoDbContext _dbContext;

        public CustomerRepository(PrensadaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int> AddCustomerAsync(Customer customer)
        {
            await _dbContext.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return customer.CustomerId;
        }

        public async Task<Customer?> GetByIdAsync(int id) => await _dbContext.Customers
            .Include(x => x.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(p => p.Product)
            .SingleOrDefaultAsync(c => c.CustomerId == id);

        public Task<List<Customer>> GetCustomersAsync() => _dbContext.Customers
            .Include(x => x.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(p => p.Product)
            .AsNoTracking()
            .ToListAsync();

        public async Task UpdateAsync(Customer customer)
        {
            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();
        }

        public Task<bool> PhoneIsExistsAsync(string phone) => _dbContext.Customers
            .AnyAsync(c => c.Phone.Equals(phone));
    }
}
