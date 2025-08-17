using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Services;
using System.Data;

namespace Prensadao.API.Controllers
{
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

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] OrderRequestDto order)
        {
            try
            {
                var result = await _orderService.OrderCreate(order);

                return Ok( new { message = "Pedido criado com sucesso.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar pedido, motivo: {ex.Message}");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _orderService.GetOrders();
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
                var result = await _orderService.GetById(id);

                if (result is null)
                    return NotFound("Pedido não encontrado.");
                else
                    return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao localizar o pedido, {ex.Message}");
            }
        }

        [HttpPut("UpdateStatus")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDTO dto)
        {
            try
            {
                var result = await _orderService.UpdateStatus(dto);
                return Ok( new { message = "Status do pedido atualizado com sucesso.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar o pedido, motivo: {ex.Message}");
            }
        }

        [HttpPut("Enabled")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Enabled([FromQuery] int id)
        {
            try
            {
                await _orderService.Enabled(id);
                return Ok("Pedido cancelado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
        }
    }
}
