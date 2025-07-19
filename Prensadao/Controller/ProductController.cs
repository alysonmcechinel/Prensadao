using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;

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
        public async Task<IActionResult> Post([FromBody] ProductRequestDto product)
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
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _productService.GetProducts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _productService.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao localizar o Produto, {ex.Message}");
            }
        }

        [HttpPost("Enabled")]
        public async Task<IActionResult> Enabled(int id)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(ProductRequestDto dto)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}
