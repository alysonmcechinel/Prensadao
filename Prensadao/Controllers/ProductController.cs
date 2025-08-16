using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Interfaces;

namespace Prensadao.API.Controllers
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
        public async Task<IActionResult> Enabled(ProductEnabledDto dto)
        {
            try
            {
                await _productService.Enabled(dto);
                return Ok($"Produto atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(ProductRequestDto dto)
        {
            try
            {
                await _productService.Update(dto);
                return Ok("Produto atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
