using Prensadao.Application.Models;
using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();
        Task<int> AddProduct(ProductDto dto);
    }
}
