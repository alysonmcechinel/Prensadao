using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;

namespace Prensadao.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] ProductRequestDto product)
        {
            try
            {
                var result = await _productService.AddProduct(product);

                return Ok(new { message = "Produto criado com sucesso.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar produto, {ex.Message}");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
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
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var result = await _productService.GetById(id);

                if (result is null)
                    return NotFound("Produto não encontrado.");
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao localizar o produto, motivo: {ex.Message}");
            }
        }

        [HttpPost("Enabled")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enabled([FromBody] ProductEnabledDto dto)
        {
            try
            {
                await _productService.Enabled(dto);
                return Ok($"Produto atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] ProductRequestDto dto)
        {
            try
            {
                await _productService.Update(dto);
                return Ok("Produto atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
