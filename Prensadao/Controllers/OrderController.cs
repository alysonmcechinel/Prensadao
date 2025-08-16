using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Interfaces;
using System.Data;

namespace Prensadao.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderRequestDto order)
        {
            try
            {
                var result = await _orderService.OrderCreate(order);

                return Ok(new
                {
                    message = "Pedido criado com sucesso",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar pedido {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _orderService.GetOrders();

            return Ok(result);
        }

        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDTO dto)
        {
            try
            {
                var result = await _orderService.UpdateStatus(dto);
                return Ok( new { message = "Status do pedido atualizado com sucesso.", data = result });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao atualizar o pedido {ex.Message}");
            }
        }

        [HttpPut("Enabled")]
        public async Task<IActionResult> Enabled(int id)
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
