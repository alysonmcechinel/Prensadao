using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;
using Prensadao.Application.Services;

namespace Prensadao.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IOrderItemService _orderItemService;

        public ProductController(IProductService productService, IOrderItemService orderItemService)
        {
            _productService = productService;
            _orderItemService = orderItemService;
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

        [HttpGet("/GetOrderItems")]
        public async Task<IActionResult> GetOrderItems()
        {
            var result = await _orderItemService.GetOrderItems();

            return Ok(result);
        }
    }
}
