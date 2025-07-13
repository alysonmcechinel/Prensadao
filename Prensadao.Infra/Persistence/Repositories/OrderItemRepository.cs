using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
