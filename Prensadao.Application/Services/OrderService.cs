using Prensadao.Application.DTOs;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;
using NodaTime;
using System.Transactions;

namespace Prensadao.Application.Services
{
    //TODO: implementar FluentValidation
    public class OrderService : IOrderService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClock _clock;

        public OrderService(
            IMessagePublisher messagePublisher,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductRepository productRepository,
            IClock clock)
        {
            _messagePublisher = messagePublisher;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _clock = clock;
        }

        public async Task<List<OrderResponseDto>> GetOrdersAsync() => OrderResponseDto.ToListDto(await _orderRepository.GetAllWithDetailsAsync());

        public async Task<OrderResponseDto> GetByIdAsync(int id)
        {
            return OrderResponseDto.ToDto(await GetOrderByIdOrThrowAsync(id));
        }

        public async Task<int> OrderCreateAsync(OrderRequestDto dto)
        {
            ValidateOrderRequest(dto);

            await ValidateOrderItemsAsync(dto.OrderItems);

            var prices = await GetPricesAsync(dto.OrderItems);
            var totalAmountOrder = CalculateTotalAmount(dto.OrderItems, prices);
            var order = CreateOrder(dto, totalAmountOrder);

            await OrderCreateAsync(dto, prices, order);
            await PublishOrderMessageAsync(order);

            return order.OrderId;
        }

        // Exemplo de metodo  Atomico
        private async Task OrderCreateAsync(OrderRequestDto dto, Dictionary<int, decimal> prices, Order order)
        {
            // 1. A GARANTIA DA ATOMICIDADE
            // O TransactionScope cria uma "bolha". Tudo que acontece aqui dentro
            // precisa funcionar, ou nada será salvo no banco ("Tudo ou Nada").
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _orderRepository.AddAsync(order);

                // 2. Operações Dependentes
                // Se ocorrer um erro neste loop (ex: erro de banco),
                // o pedido criado na linha acima será revertido (Rollback) automaticamente.
                foreach (var item in dto.OrderItems)
                {
                    var unitPrice = prices[item.ProductId];
                    var orderItem = new OrderItem(item.Quantity, unitPrice, order.OrderId, item.ProductId);
                    await _orderItemRepository.AddAsync(orderItem);
                }

                // 3. O "Commit" Final
                // Apenas se o código chegar nesta linha, os dados são persistidos.
                // Se sair do 'using' sem passar aqui, tudo é cancelado.
                scope.Complete();
            };
        }

        public async Task<OrderResponseDto> UpdateStatusAsync(UpdateStatusDto dto)
        {
            ValidateUpdateStatusRequest(dto);

            var order = await GetOrderByIdOrThrowAsync(dto.OrderId);

            if (order.Status == dto.Status)
                return OrderResponseDto.ToDto(order);

            order.ChangeStatus(dto.Status!.Value);
            await _orderRepository.UpdateAsync(order);
            await PublishNotifyMessageAsync(order);

            return OrderResponseDto.ToDto(order);
        }        

        public async Task CancelAsync(int id)
        {
            var order = await GetOrderByIdOrThrowAsync(id);

            order.Cancel();

            await _orderRepository.UpdateAsync(order);
            await PublishNotifyMessageAsync(order);
        }

        // Privates
        private static void ValidateOrderRequest(OrderRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado.");

            if (!dto.IsDelivery.HasValue)
                throw new ArgumentException("A modalidade de entrega do pedido deve ser informada.");
        }

        private static void ValidateUpdateStatusRequest(UpdateStatusDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.OrderId <= 0)
                throw new ArgumentException("Id do pedido deve ser maior que zero.");

            if (!dto.Status.HasValue)
                throw new ArgumentException("Status do pedido deve ser informado.");

            if (!Enum.IsDefined(dto.Status.Value))
                throw new ArgumentException("Status do pedido inválido.");
        }

        private Order CreateOrder(OrderRequestDto dto, decimal totalAmountOrder)
            => new(dto.IsDelivery!.Value, totalAmountOrder, dto.Notes ?? string.Empty, dto.CustomerId, _clock.NowUtc());

        private static decimal CalculateTotalAmount(IEnumerable<OrderItemRequestDto> items, IReadOnlyDictionary<int, decimal> prices)
            => Math.Round(items.Sum(item => prices[item.ProductId] * item.Quantity), 2, MidpointRounding.AwayFromZero);

        private async Task<Order> GetOrderByIdOrThrowAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order is null)
                throw new ArgumentException("Pedido não encontrado.");

            return order;
        }

        private async Task<Dictionary<int, decimal>> GetPricesAsync(IEnumerable<OrderItemRequestDto> orderItems)
        {
            var productIds = orderItems.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.GetValuesByIdsAsync(productIds);
            var prices = products.ToDictionary(p => p.ProductId, p => p.Value);

            var idsNotFound = productIds.Except(prices.Keys).ToList();
            if (idsNotFound.Any())
                throw new ArgumentException($"Produtos não encontrados: {string.Join(", ", idsNotFound)}");

            return prices;
        }

        private async Task ValidateOrderItemsAsync(IEnumerable<OrderItemRequestDto> orderItems)
        {
            var items = orderItems.ToList();

            if (!items.Any())
                throw new ArgumentException("Pedido não pode ser feito sem itens.");

            if (items.Any(x => x.ProductId <= 0))
                throw new ArgumentException("Pedido contém itens com ProductId inválido.");

            if (items.Any(x => x.Quantity <= 0))
                throw new ArgumentException("Pedido contém itens com quantidade inválida.");

            var productIds = items.Select(x => x.ProductId).Distinct().ToList();
            var verifyProductActive = await _productRepository.ExistsInactiveByIdsAsync(productIds);
            if (verifyProductActive)
                throw new ArgumentException("Pedido não pode ser feito com produtos inativos.");
        }

        private Task PublishOrderMessageAsync(Order order)
            => _messagePublisher.PublishAsync(CreateOrderMessage(order), RabbitMqConstants.Exchanges.OrderExchange);

        private Task PublishNotifyMessageAsync(Order order)
            => _messagePublisher.PublishAsync(CreateNotifyMessage(order), RabbitMqConstants.Exchanges.NotifyExchange);

        private static OrderMessageDto CreateOrderMessage(Order order)
            => new()
            {
                OrderId = order.OrderId
            };

        private static NotifyMessageDto CreateNotifyMessage(Order order)
            => new()
            {
                OrderId = order.OrderId,
                CustomerName = order.Customer.Name,
                IsDelivery = order.IsDelivery,
                OrderStatus = order.Status,
                Phone = order.Customer.Phone
            };
    }
}
