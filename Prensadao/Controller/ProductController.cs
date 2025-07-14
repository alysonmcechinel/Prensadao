using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;

namespace Prensadao.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto product)
        {
            try
            {
                var result = await _productService.AddProduct(product);

                return Ok(new
                {
                    message = "Produto criado com sucesso",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar Produto, {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _productService.GetProducts();

            return Ok(result);
        }
    }
}
