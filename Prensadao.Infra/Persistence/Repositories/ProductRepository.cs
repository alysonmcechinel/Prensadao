using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;
using Prensadao.Domain.Views;

namespace Prensadao.Infra.Persistence.Repositories
{
    internal class ProductRepository : IProductRepository
    {
        private readonly PrensadaoDbContext _dbContext;

        public ProductRepository(PrensadaoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return product.ProductId;
        }

        public async Task UpdateAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        public Task<Product?> GetByIdAsync(int id) => _dbContext.Products
            .SingleOrDefaultAsync(x => x.ProductId == id);

        public Task<List<Product>> GetAllAsync() => _dbContext.Products
            .AsNoTracking()
            .ToListAsync();

        public Task<bool> ExistsByNameAsync(string name) => _dbContext.Products
            .AnyAsync(product => product.Name == name);

        public Task<bool> ExistsInactiveByIdsAsync(IReadOnlyCollection<int> productIds) => _dbContext.Products
            .AnyAsync(product => productIds.Contains(product.ProductId) && !product.Enabled);

        public Task<List<ProductValueModels>> GetValuesByIdsAsync(IReadOnlyCollection<int> ids) => _dbContext.Products
            .Where(product => ids.Contains(product.ProductId))
            .Select(product => new ProductValueModels(product.ProductId, product.Value))
            .AsNoTracking()
            .ToListAsync();
    }
}
