using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Persistence.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly PrensadaoDbContext _dbContext;

        public OrderItemRepository(PrensadaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(OrderItem orderItem)
        {
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();
        }

        public Task<List<OrderItem>> GetAllAsync() => _dbContext.OrderItems
            .Include(x => x.Order)
            .Include(x => x.Product)
            .AsNoTracking()
            .ToListAsync();
    }
}
