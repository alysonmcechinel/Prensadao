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

        public async Task AddOrderItem(OrderItem ordemItem)
        {
            await _dbContext.AddAsync(ordemItem);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<OrderItem>> GetOrderItems() => await _dbContext.OrderItems
            .Include(x => x.Order)
            .Include(x => x.Product)
            .ToListAsync();
    }
}
