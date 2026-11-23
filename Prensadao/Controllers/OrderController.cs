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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("Post")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody] OrderRequestDto dto)
        {
            var result = await _orderService.OrderCreateAsync(dto);

            return Ok(new
            {
                message = "Pedido criado com sucesso.",
                data = result
            });
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _orderService.GetOrdersAsync();
            return Ok(result);
        }

        [HttpGet("GetById")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] int id)
        {
            var result = await _orderService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("Status")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateStatusAsync([FromBody] UpdateStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(dto);

            return Ok(new
            {
                message = "Status do pedido atualizado com sucesso.",
                data = result
            });
        }

        [HttpPost("{id:int}/Cancel")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelAsync([FromRoute] int id)
        {
            await _orderService.CancelAsync(id);
            return Ok("Pedido cancelado com sucesso.");
        }
    }
}
