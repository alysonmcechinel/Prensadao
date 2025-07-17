using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProducts();
        Task<int> AddProduct(Product product);
    }
}
