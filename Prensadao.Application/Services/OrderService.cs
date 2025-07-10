using Prensadao.Application.Interfaces;
using Prensadao.Application.Models;
using Prensadao.Application.Publish;

namespace Prensadao.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBus _bus;

        public OrderService(IBus bus)
        {
            _bus = bus;
        }

        public async Task<OrderResultViewModel> OrderCreate(OrderDto dto)
        {
            if (dto is null)
                throw new ArgumentException("O pedido não pode ser nulo");

            if (!dto.Items.Any())
                throw new ArgumentException("Pedido não pode ser feito sem items");


            await _bus.Publish(dto, RabbitMqConstants.Exchanges.OrderExchange);

            return new OrderResultViewModel(); // ajustar depois
        }
    }
}
