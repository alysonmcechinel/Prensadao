using Prensadao.Domain.Entities;
using Prensadao.Domain.Views;

namespace Prensadao.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<int> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsInactiveByIdsAsync(IReadOnlyCollection<int> productIds);
        Task<List<ProductValueModels>> GetValuesByIdsAsync(IReadOnlyCollection<int> ids);
    }
}
