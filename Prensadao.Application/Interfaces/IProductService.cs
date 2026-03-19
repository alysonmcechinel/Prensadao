using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces
{
    public interface IProductService
    {
        Task<int> AddProductAsync(ProductRequestDto dto);
        Task UpdateAsync(ProductRequestDto dto);
        Task<ProductResponseDto> GetByIdAsync(int id);
        Task<List<ProductResponseDto>> GetProductsAsync();
        Task EnabledAsync(ProductEnabledDto dto);
    }
}
