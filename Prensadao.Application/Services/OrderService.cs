using Prensadao.Application.DTOs;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;
using System.Transactions;

namespace Prensadao.Application.Services
{
    //TODO: implementar FluentValidation
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

            if (order.Status == dto.OrderStatus)
                throw new ArgumentException("Status do pedido já está definido como o informado.");

            order.SetStatus(dto.OrderStatus);
            await _orderRepository.UpdateAsync(order);
            await PublishNotifyMessageAsync(order);

            return OrderResponseDto.ToDto(order);
        }        

        public async Task EnabledAsync(int id)
        {
            var order = await GetOrderByIdOrThrowAsync(id);

            ValidateOrderCanBeCanceled(order);
            order.SetStatus(OrderStatusEnum.Cancelado);

            await _orderRepository.UpdateAsync(order);
            await PublishNotifyMessageAsync(order);
        }

        // Privates
        private static void ValidateOrderRequest(OrderRequestDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            if (dto.CustomerId <= 0)
                throw new ArgumentException("Pedido não pode ser feito sem cliente cadastrado.");
        }

        private static void ValidateUpdateStatusRequest(UpdateStatusDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
        }

        private static Order CreateOrder(OrderRequestDto dto, decimal totalAmountOrder)
            => new(dto.Delivery, totalAmountOrder, dto.Observation ?? string.Empty, dto.CustomerId, NodaTimeExtensions.NowUtc());

        private static decimal CalculateTotalAmount(IEnumerable<OrderItemRequestDto> items, IReadOnlyDictionary<int, decimal> prices)
            => Math.Round(items.Sum(item => prices[item.ProductId] * item.Quantity), 2, MidpointRounding.AwayFromZero);

        private async Task<Order> GetOrderByIdOrThrowAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);

            if (order is null)
                throw new ArgumentException("Pedido não encontrado.");

            return order;
        }

        private static void ValidateOrderCanBeCanceled(Order order)
        {
            if (order.Status != OrderStatusEnum.EmPreparacao && order.Status != OrderStatusEnum.Criado)
                throw new ArgumentException($"Pedido não pode ser cancelado pois, já esta com status: {order.Status.GetDescription()}");
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
            => _bus.Publish(CreateOrderMessage(order), RabbitMqConstants.Exchanges.OrderExchange);

        private Task PublishNotifyMessageAsync(Order order)
            => _bus.Publish(CreateNotifyMessage(order), RabbitMqConstants.Exchanges.NotifyExchange);

        private static OrderMessageDto CreateOrderMessage(Order order)
            => new()
            {
                OrderId = order.OrderId
            };

        private static NotifyMessageDto CreateNotifyMessage(Order order)
            => new()
            {
                OrderId = order.OrderId,
                ConsumerName = order.Customer.Name,
                Delivery = order.IsDelivery,
                OrderStatus = order.Status,
                Phone = order.Customer.Phone
            };
    }
}
