using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;
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

        public async Task<int> AddProduct(ProductDto dto)
        {
            var product = new Product(dto.Name, dto.Value, dto.Description);

            return await _productRepository.AddProduct(product);
        }

        public async Task<List<Product>> GetProducts() => await _productRepository.GetProducts();
    }
}
