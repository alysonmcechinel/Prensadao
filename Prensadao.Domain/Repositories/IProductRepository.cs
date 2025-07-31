using Prensadao.Domain.DTOs;
using Prensadao.Domain.Entities;

namespace Prensadao.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<int> AddProduct(Product product);
        Task Update(Product product);
        Task<Product> GetById(int id);
        Task<List<Product>> GetProducts();
        Task<bool> NameAlreadyExists(string name);
        Task<bool> ExistsInactiveProduct(List<int> productsIDs);
        Task<List<ProductValueDTO>> ValueOfProducts(List<int> ids);
    }
}
