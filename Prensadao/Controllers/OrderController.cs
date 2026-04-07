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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("Post")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody] OrderRequestDto dto)
        {
            try
            {
                var result = await _orderService.OrderCreateAsync(dto);

                return Ok(new
                {
                    message = "Pedido criado com sucesso.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar pedido, motivo: {ex.Message}");
            }
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _orderService.GetOrdersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetById")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] int id)
        {
            try
            {
                var result = await _orderService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateStatus")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] UpdateStatusDto dto)
        {
            try
            {
                var result = await _orderService.UpdateStatusAsync(dto);

                return Ok(new
                {
                    message = "Status do pedido atualizado com sucesso.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar o pedido, motivo: {ex.Message}");
            }
        }

        [HttpPut("Enabled")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnabledAsync([FromQuery] int id)
        {
            try
            {
                await _orderService.EnabledAsync(id);
                return Ok("Pedido cancelado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
