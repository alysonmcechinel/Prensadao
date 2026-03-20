using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Services
{
    //TODO: implementar FluentValidation
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<int> AddProductAsync(ProductRequestDto dto)
        {
            ValidateProductRequest(dto);
            await EnsureProductNameIsAvailableAsync(dto.Name);

            return await _productRepository.AddAsync(CreateProduct(dto));
        }

        public async Task<ProductResponseDto> GetByIdAsync(int id)
        {
            ValidateProductId(id);
            return ProductResponseDto.ToDto(await GetProductByIdOrThrowAsync(id));
        }

        public async Task UpdateAsync(ProductRequestDto dto)
        {
            ValidateProductRequest(dto);
            ValidateProductId(dto.ProductId);

            var product = await GetProductByIdOrThrowAsync(dto.ProductId!.Value);
            UpdateProduct(product, dto);
            await _productRepository.UpdateAsync(product);
        }

        public async Task<List<ProductResponseDto>> GetProductsAsync() => ProductResponseDto.ToListDto(await _productRepository.GetAllAsync());

        public async Task EnabledAsync(ProductEnabledDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var product = await GetProductByIdOrThrowAsync(dto.ProductId);

            if (product.Enabled == dto.Enabled)
                return;

            product.EnabledProduct(dto.Enabled);
            await _productRepository.UpdateAsync(product);
        }

        private static Product CreateProduct(ProductRequestDto dto)
            => new(dto.Name, dto.Value, dto.Description);

        private static void UpdateProduct(Product product, ProductRequestDto dto)
            => product.Update(dto.Name, dto.Enabled, dto.Value, dto.Description);

        private static void ValidateProductRequest(ProductRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("O produto deve ter um nome.");

            if (dto.Value <= 0)
                throw new ArgumentException("O valor do produto deve ser maior que 0");
        }

        private static void ValidateProductId(int? productId)
        {
            if (!productId.HasValue || productId <= 0)
                throw new ArgumentException("O ID informado está incorreto.");
        }

        private static void ValidateProductId(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("O ID informado está incorreto.");
        }

        private async Task<Product> GetProductByIdOrThrowAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);

            if (product is null)
                throw new ArgumentException("Produto não encontrado.");

            return product;
        }

        private async Task EnsureProductNameIsAvailableAsync(string productName)
        {
            var nameAlreadyExists = await _productRepository.ExistsByNameAsync(productName);
            if (nameAlreadyExists)
                throw new ArgumentException("Já existe um produto com esse nome");
        }
    }
}
