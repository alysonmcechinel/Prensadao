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
        public async Task<int> AddCustomer(Customer customer)
        {
            await _dbContext.AddAsync(customer);
            await _dbContext.SaveChangesAsync();

            return customer.CustomerId;
        }

        public async Task<List<Customer>> GetCustomers() => await _dbContext.Customers
            .Include(x => x.Orders)
            .ToListAsync();
    }
}
