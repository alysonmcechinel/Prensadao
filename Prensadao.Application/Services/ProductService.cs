using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<int> AddProduct(ProductRequestDto dto)
        {
            if(string.IsNullOrEmpty(dto.Name))
                throw new ArgumentException("O produto deve ter um nome.");

            bool nameAlreadyExists = await _productRepository.NameAlreadyExists(dto.Name);
            if (nameAlreadyExists)
                throw new ArgumentException("Já existe um produto com esse nome");

            if (dto.Value <= 0)
                throw new ArgumentException("O valor do produto deve ser maior que 0");

            var product = new Product(dto.Name, dto.Value, dto.Description);

            return await _productRepository.AddProduct(product);
        }

        public async Task<ProductResponseDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID informado incorretamente");
            
             var product = await _productRepository.GetById(id);

            if (product == null)
                throw new ArgumentException("Produto não encontrado");

            return ProductResponseDto.ToDto(product);
        }

        public async Task Update(ProductRequestDto dto)
        {
            if (dto.ProductId <= 0)
                throw new ArgumentException("O ID informado incorretamente");

            var product = await _productRepository.GetById(dto.ProductId!.Value);

            if (product == null)
                throw new ArgumentException("Produto não encontrado");

            product.Update(dto.Name, dto.Enabled, dto.Value, dto.Description);
            await _productRepository.Update(product);
        }

        public async Task<List<ProductResponseDto>> GetProducts() => ProductResponseDto.ToListDto(await _productRepository.GetProducts());

        public async Task Enabled(ProductEnabledDto dto)
        {
            var product = await _productRepository.GetById(dto.ProductId);

            if (product == null)
                throw new ArgumentException("Produto não encontrado");

            if (product.Enabled != dto.Enabled)
                product.EnabledProduct(dto.Enabled);
            else
                return;

            await _productRepository.Update(product);
        }
    }
}
