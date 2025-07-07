using Microsoft.AspNetCore.Mvc;
using Prensadao.Application;
using Prensadao.Application.Models;
using Prensadao.Application.Publish;

namespace Prensadao.API.Controller
{
    [ApiController]
    [Route("/")]
    public class OrderController : ControllerBase
    {
        private readonly IBus _bus;

        public OrderController(IBus bus)
        {
            _bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OrderDto order)
        {
            try
            {
                await _bus.Publish(order, RabbitMqConstants.Exchanges.OrderExchange);
                return Ok("Mensagem publicada");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}
