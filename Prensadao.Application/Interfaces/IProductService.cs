using Prensadao.Application.Models.Request;
using Prensadao.Application.Models.Response;
using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IProductService
    {
        Task<int> AddProduct(ProductRequestDto dto);
        Task<ProductResponseDto> GetById(int id);
        Task<List<ProductResponseDto>> GetProducts();
    }
}
