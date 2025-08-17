using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;

namespace Prensadao.API.Controllers
{
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

        [HttpPost]
        [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] CustomerRequestDto dto)
        {
            try
            {
                var result = await _customerService.AddCustomer(dto);

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
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var result = await _customerService.GetById(id);

                if (result is null)
                    return NotFound("Cliente não encontrado.");
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _customerService.GetCustomers();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(CustomerRequestDto dto)
        {
            try
            {
                await _customerService.Update(dto);
                return Ok("Cliente atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
