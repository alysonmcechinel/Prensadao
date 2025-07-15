using Prensadao.Application.Models.Request;
using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
        Task<int> AddProduct(ProductDto dto);
    }
}
