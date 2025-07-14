using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Persistence.Repositories
{
    internal class ProductRepository : IProductRepository
    {
        private readonly PrensadaoDbContext _dbContext;

        public ProductRepository(PrensadaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddProduct(Product product)
        {
            await _dbContext.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return product.ProductId;
        }

        public async Task<List<Product>> GetProducts() => await _dbContext.Products
            .Include(x => x.OrderItems)
            .ToListAsync();
    }
}
