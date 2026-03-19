using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;

namespace Prensadao.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionGenerator _promotionGenerator;

        public PromotionsController(IPromotionGenerator promotionGenerator)
        {
            _promotionGenerator = promotionGenerator;
        }

        [HttpPost("Generate")]
        [ProducesResponseType(typeof(PromotionProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateAsync([FromBody] GeneratePromotionRequest dto, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _promotionGenerator.GenerateWeeklyPromotionAsync(dto, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar promoção, {ex.Message}");
            }
        }
    }
}
