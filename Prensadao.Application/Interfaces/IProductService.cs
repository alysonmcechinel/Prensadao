using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Domain.Entities;

namespace Prensadao.Application.Interfaces
{
    public interface IProductService
    {
        Task<int> AddProduct(ProductRequestDto dto);
        Task Update(ProductRequestDto dto);
        Task<ProductResponseDto> GetById(int id);
        Task<List<ProductResponseDto>> GetProducts();
        Task Enabled(ProductEnabledDto dto);
    }
}
