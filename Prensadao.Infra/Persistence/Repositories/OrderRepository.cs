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

        public async Task<int> CreateOrder(Order order)
        {
            await _dbContext.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            return order.OrderId;
        }

        public async Task<List<Order>> GetOrders() => 
            await _dbContext.Orders
                .Include(x => x.Customer)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsNoTracking()
                .ToListAsync();
         
    }
}
