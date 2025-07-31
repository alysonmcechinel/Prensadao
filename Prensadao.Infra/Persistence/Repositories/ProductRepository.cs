using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.DTOs;
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

        public async Task Update(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Product?> GetById(int id) => await _dbContext.Products
            .SingleOrDefaultAsync(x => x.ProductId == id);

        public async Task<List<Product>> GetProducts() => await _dbContext.Products
            .Include(x => x.OrderItems)
            .ToListAsync();        

        public async Task<bool> NameAlreadyExists(string name) => await _dbContext.Products.AnyAsync(x => x.Name == name);

        public async Task<bool> ExistsInactiveProduct(List<int> productsIDs) => await _dbContext.Products.AnyAsync(x => productsIDs.Contains(x.ProductId) && !x.Enabled);

        public async Task<List<ProductValueDTO>> ValueOfProducts(List<int> ids) => await _dbContext.Products
            .Where(x => ids.Contains(x.ProductId))
            .Select(x => new ProductValueDTO
                {
                    ProductId = x.ProductId,
                    Value = x.Value
                })
            .ToListAsync();
    }
}
