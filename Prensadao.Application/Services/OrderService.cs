using Prensadao.Application.DTOs;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;
using System.Threading.Tasks;
using System.Transactions;

namespace Prensadao.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBus _bus;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IBus bus, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, IProductRepository productRepository)
        {
            _bus = bus;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
        }

        public async Task<List<OrderReponseDto>> GetOrders()
        {
            var result = await _orderRepository.GetOrders();
            return OrderReponseDto.ToListDto(result);
        }

        public async Task<int> OrderCreate(OrderRequestDto dto)
        {
            if (dto is null)
                throw new ArgumentException("O pedido não pode ser nulo.");

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado.");

            await ValidationsOrderItem(dto);
            Dictionary<int, decimal> prices = await GetPrices(dto);

            decimal totalAmountOrder = Math.Round(dto.OrderItems.Sum(i => prices[i.ProductId] * i.Quantity));

            var order = new Order(dto.Delivery, totalAmountOrder, dto.Observation, dto.CustomerId);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _orderRepository.CreateOrder(order);

                foreach (var item in dto.OrderItems)
                {
                    var unitPrice = prices[item.ProductId];
                    var orderItem = new OrderItem(item.Quantity, unitPrice, order.OrderId, item.ProductId);
                    await _orderItemRepository.AddOrderItem(orderItem);
                }

                scope.Complete();
            };           

            await Message(order);

            return order.OrderId;
        }

        public async Task<OrderReponseDto> UpdateStatus(UpdateStatusDTO dto)
        {
            var order = await _orderRepository.GetById(dto.OrderId);

            if (order is null)
                throw new ArgumentException("Pedido não encontrado.");

            if (order.OrderStatus != dto.OrderStatus)
            {
                order.UpdateStatus(dto.OrderStatus);
                await _orderRepository.Update(order);
                await MessageNotify(order);

                return OrderReponseDto.ToDto(order);
            }
            else
                throw new ArgumentException("Status não pode ser atualizado");
        }        

        public async Task Enabled(int id)
        {
            var order = await _orderRepository.GetById(id);

            if (order is null)
                throw new ArgumentException("Pedido não encontrado.");

            if (order.OrderStatus == OrderStatusEnum.EmPreparacao || order.OrderStatus == OrderStatusEnum.Criado)
                order.UpdateStatus(OrderStatusEnum.Cancelado);
            else
                throw new ArgumentException($"Pedido não pode ser cancelado pois, já esta com status: {order.OrderStatus.GetDescription()}");

            await _orderRepository.Update(order);
            await MessageNotify(order);
        }

        // Privates
        private async Task<Dictionary<int, decimal>> GetPrices(OrderRequestDto dto)
        {
            var productIds = dto.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.ValueOfProducts(productIds);
            var prices = products.ToDictionary(p => p.ProductId, p => p.Value);

            var idsNotFound = productIds.Except(prices.Keys).ToList();
            if (idsNotFound.Any())
                throw new ArgumentException($"Produtos não encontrados: {string.Join(", ", idsNotFound)}");

            return prices;
        }

        private async Task ValidationsOrderItem(OrderRequestDto dto)
        {
            if (!dto.OrderItems.Any())
                throw new ArgumentException("Pedido não pode ser feito sem items.");

            if (dto.OrderItems.Any(x => x.ProductId <= 0))
                throw new ArgumentException("Pedido contém Id inválido.");

            if (dto.OrderItems.Any(x => x.Quantity <= 0))
                throw new ArgumentException("Pedido contém itens com quantidade inválida.");

            List<int> productsIDs = dto.OrderItems.Select(x => x.ProductId).ToList();
            var verifyProductActive = await _productRepository.ExistsInactiveProduct(productsIDs);
            if (verifyProductActive)
                throw new ArgumentException("Pedido não pode ser feito com produtos inativos.");
        }

        private async Task Message(Order order)
        {
            var messageDto = new OrderMessageDto
            {
                OrderId = order.OrderId
            };

            await _bus.Publish(messageDto, RabbitMqConstants.Exchanges.OrderExchange);
        }

        private async Task MessageNotify(Order order)
        {
            var notify = new NotifyMessageDto
            {
                OrderId = order.OrderId,
                ConsumerName = order.Customer.Name,
                Delivery = order.Delivery,
                OrderStatus = order.OrderStatus,
                Phone = order.Customer.Phone
            };

            await _bus.Publish(notify, RabbitMqConstants.Exchanges.NotifyExchange);
        }
    }
}
