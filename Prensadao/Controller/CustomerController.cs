using Microsoft.AspNetCore.Mvc;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Models.Request;

namespace Prensadao.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerDto dto)
        {
            try
            {
                var result = await _customerService.AddCustomer(dto);

                return Ok(new
                {
                    message = "Cadastro efetuado com sucesso",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao criar cadastro. {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _customerService.GetCustomers();

            return Ok(result);
        }
    }
}
