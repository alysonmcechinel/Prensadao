using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PrensadaoDbContext _dbContext;
        public OrderRepository(PrensadaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddAsync(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            return order.OrderId;
        }

        public async Task UpdateAsync(Order order)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
        }

        public Task<Order?> GetByIdAsync(int id) => _dbContext.Orders
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order => order.OrderId == id);

        public Task<Order?> GetByIdWithDetailsAsync(int id) => BuildOrderDetailsQuery()
            .SingleOrDefaultAsync(order => order.OrderId == id);

        public Task<List<Order>> GetAllWithDetailsAsync() => BuildOrderDetailsQuery()
            .AsNoTracking()
            .ToListAsync();

        private IQueryable<Order> BuildOrderDetailsQuery() => _dbContext.Orders
            .Include(x => x.OrderItems)
                .ThenInclude(o => o.Product)
            .Include(x => x.Customer)
            .AsSplitQuery();
    }
}
