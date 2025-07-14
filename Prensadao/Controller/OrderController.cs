using Microsoft.AspNetCore.Mvc;
using Prensadao.Application;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;
using Prensadao.Application.Publish;

namespace Prensadao.API.Controller
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
        public async Task<IActionResult> Post([FromBody] OrderDto order)
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

    }
}
