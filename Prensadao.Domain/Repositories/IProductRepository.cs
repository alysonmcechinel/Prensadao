using Prensadao.Domain.Entities;
using Prensadao.Domain.Views;

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
        Task<List<ProductValueModels>> ValueOfProducts(List<int> ids);
    }
}
