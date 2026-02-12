using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;

namespace Prensadao.API.Controllers
{
    //TODO: autenticacao e respostas.
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("Post")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody] CustomerRequestDto dto)
        {
            try
            {
                var result = await _customerService.AddCustomerAsync(dto);

                return Ok(new
                {
                    message = "Cliente cadastrado com sucesso.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao cadastrar, motivo: {ex.Message}");
            }
        }

        [HttpGet("GetById")]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] int id)
        {
            try
            {
                var result = await _customerService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _customerService.GetCustomersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        [HttpPut("Update")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync(CustomerRequestDto dto)
        {
            try
            {
                await _customerService.UpdateAsync(dto);
                return Ok("Cliente atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
