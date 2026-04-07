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

        public async Task<int> AddAsync(Customer customer)
        {
            await _dbContext.Customers.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return customer.CustomerId;
        }

        public Task<Customer?> GetByIdAsync(int id) => _dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.CustomerId == id);

        public Task<Customer?> GetByIdWithDetailsAsync(int id) => BuildCustomerDetailsQuery()
            .SingleOrDefaultAsync(customer => customer.CustomerId == id);

        public Task<List<Customer>> GetAllWithDetailsAsync() => BuildCustomerDetailsQuery()
            .AsNoTracking()
            .ToListAsync();

        public async Task UpdateAsync(Customer customer)
        {
            _dbContext.Customers.Update(customer);
            await _dbContext.SaveChangesAsync();
        }

        public Task<bool> ExistsByPhoneAsync(string phone) => _dbContext.Customers
            .AnyAsync(customer => customer.Phone == phone);

        private IQueryable<Customer> BuildCustomerDetailsQuery() => _dbContext.Customers
            .Include(x => x.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(p => p.Product)
            .AsSplitQuery();
    }
}
