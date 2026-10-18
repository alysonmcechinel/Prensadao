using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;
using Prensadao.Domain.Views;

namespace Prensadao.Test.Application;

public class OrderServiceTest
{
    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveCriarPedidoItensEPublicarMensagem_QuandoSucesso(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IOrderItemRepository orderItemRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService)
    {
        // arrange
        var dto = new OrderRequestDto
        {
            CustomerId = 1,
            IsDelivery = true,
            Notes = "Sem cebola",
            OrderItems =
            [
                new OrderItemRequestDto { ProductId = 10, Quantity = 2 },
                new OrderItemRequestDto { ProductId = 20, Quantity = 1 }
            ]
        };

        var products = new List<ProductValueModels>
        {
            new(10, 15.50m),
            new(20, 8.00m)
        };

        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).Returns(false);
        A.CallTo(() => productRepository.GetValuesByIdsAsync(A<IReadOnlyCollection<int>>.That.Matches(ids =>
            ids.Count == 2 &&
            ids.Contains(10) &&
            ids.Contains(20)))).Returns(products);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._))
            .Invokes((Order order) => typeof(Order).GetProperty("OrderId")!.SetValue(order, 123));

        // act
        var result = await orderService.OrderCreateAsync(dto);

        // assert
        Assert.Equal(123, result);

        A.CallTo(() => orderRepository.AddAsync(
            A<Order>.That.Matches(order =>
                order.CustomerId == dto.CustomerId &&
                order.IsDelivery == dto.IsDelivery &&
                order.Notes == dto.Notes &&
                order.TotalAmount == 39.00m &&
                order.Status == OrderStatusEnum.Criado)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderItemRepository.AddAsync(
            A<OrderItem>.That.Matches(item =>
                item.OrderId == 123 &&
                item.ProductId == 10 &&
                item.Quantity == 2 &&
                item.UnitPrice == 15.50m)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderItemRepository.AddAsync(
            A<OrderItem>.That.Matches(item =>
                item.OrderId == 123 &&
                item.ProductId == 20 &&
                item.Quantity == 1 &&
                item.UnitPrice == 8.00m)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => messagePublisher.PublishAsync(
            A<OrderMessageDto>.That.Matches(message => message.OrderId == 123),
            RabbitMqConstants.Exchanges.OrderExchange,
            A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_PedidoNaoPodeSerNulo(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService)
    {
        // arrange
        OrderRequestDto? dto = null;

        // act
        await Assert.ThrowsAsync<ArgumentNullException>(() => orderService.OrderCreateAsync(dto!));

        // assert
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_PedidoSemClienteCadastrado(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.CustomerId = 0;

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("Pedido não pode ser feito sem cliente cadastrado.", ex.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_ShouldThrowAndNotPersist_WhenDeliveryModeIsMissing(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.CustomerId = 1;
        dto.IsDelivery = null;

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("A modalidade de entrega do pedido deve ser informada.", exception.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_SemItens(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.OrderItems = new List<OrderItemRequestDto>();

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("Pedido não pode ser feito sem itens.", ex.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComProdutoIdInvalido(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 0, Quantity = 1 }
        };

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("Pedido contém itens com ProductId inválido.", ex.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComQuantidadeInvalida(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 10, Quantity = 0 }
        };

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("Pedido contém itens com quantidade inválida.", ex.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComProdutoInativo(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 10, Quantity = 1 }
        };

        A.CallTo(() => productRepository.ExistsInactiveByIdsAsync(A<IReadOnlyCollection<int>>._)).Returns(true);

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // assert
        Assert.Equal("Pedido não pode ser feito com produtos inativos.", ex.Message);
        A.CallTo(() => orderRepository.AddAsync(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_DeveAtualizarPedidoPublicarNotificacaoERetornarPedido_QuandoStatusValido(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId,
        int customerId)
    {
        // arrange
        var customer = new Customer("Nome", "48999999999", "Rua", "Bairro", "123", "Cidade", "Ponto de referencia", 88000000);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(customer, customerId);

        var order = new Order(isDelivery: true, totalAmount: 42.50m, notes: "Sem cebola", customerId: customerId, createdAt: NodaTimeExtensions.NowUtc());
        typeof(Order).GetProperty("OrderId")!.SetValue(order, orderId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);

        var dto = new UpdateStatusDto
        {
            OrderId = orderId,
            Status = OrderStatusEnum.EmPreparacao
        };

        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns(order);

        // act
        var result = await orderService.UpdateStatusAsync(dto);

        // assert
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(OrderStatusEnum.EmPreparacao.GetDescription(), result.Status);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(customer.Name, result.CustomerName);
        Assert.Equal(OrderStatusEnum.EmPreparacao, order.Status);

        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderRepository.UpdateAsync(
                A<Order>.That.Matches(updatedOrder =>
                    updatedOrder.OrderId == orderId &&
                    updatedOrder.Status == OrderStatusEnum.EmPreparacao)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>.That.Matches(message =>
                    message.OrderId == orderId &&
                    message.CustomerName == customer.Name &&
                    message.IsDelivery == order.IsDelivery &&
                    message.OrderStatus == OrderStatusEnum.EmPreparacao &&
                    message.Phone == customer.Phone),
                RabbitMqConstants.Exchanges.NotifyExchange,
                A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldReturnWithoutWritingOrPublishing_WhenStatusIsAlreadySet(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId,
        int customerId)
    {
        // arrange
        var customer = new Customer("Nome", "48999999999", "Rua", "Bairro", "123", "Cidade", "Ponto de referencia", 88000000);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(customer, customerId);

        var order = new Order(isDelivery: true, totalAmount: 42.50m, notes: "Sem cebola", customerId: customerId, createdAt: NodaTimeExtensions.NowUtc());
        typeof(Order).GetProperty("OrderId")!.SetValue(order, orderId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);
        order.AdvanceStatus();
        order.AdvanceStatus();

        var dto = new UpdateStatusDto
        {
            OrderId = orderId,
            Status = OrderStatusEnum.Pronto
        };

        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns(order);

        // act
        var result = await orderService.UpdateStatusAsync(dto);

        // assert
        Assert.Equal(OrderStatusEnum.Pronto.GetDescription(), result.Status);

        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._))
            .MustNotHaveHappened();

        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldThrowAndNotReadOrWrite_WhenStatusIsUndefined(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId)
    {
        // arrange
        var dto = new UpdateStatusDto
        {
            OrderId = orderId,
            Status = (OrderStatusEnum)999
        };

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.UpdateStatusAsync(dto));

        // assert
        Assert.Equal("Status do pedido inválido.", exception.Message);
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldThrowAndNotReadOrWrite_WhenOrderIdIsInvalid(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService)
    {
        // arrange
        var dto = new UpdateStatusDto
        {
            OrderId = 0,
            Status = OrderStatusEnum.EmPreparacao
        };

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.UpdateStatusAsync(dto));

        // assert
        Assert.Equal("Id do pedido deve ser maior que zero.", exception.Message);
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldThrowAndNotReadOrWrite_WhenStatusIsMissing(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId)
    {
        // arrange
        var dto = new UpdateStatusDto { OrderId = orderId, Status = null };

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.UpdateStatusAsync(dto));

        // assert
        Assert.Equal("Status do pedido deve ser informado.", exception.Message);
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(A<int>._)).MustNotHaveHappened();
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldThrowAndNotWriteOrPublish_WhenTransitionSkipsStatus(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId)
    {
        // arrange
        var order = new Order(isDelivery: true, totalAmount: 10m, notes: string.Empty, customerId: 1, createdAt: NodaTimeExtensions.NowUtc());
        var dto = new UpdateStatusDto { OrderId = orderId, Status = OrderStatusEnum.Pronto };
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns(order);

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.UpdateStatusAsync(dto));

        // assert
        Assert.Equal("Transição de status inválida: Criado para Pronto.", exception.Message);
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task UpdateStatusAsync_ShouldThrowAndNotWriteOrPublish_WhenCancellationUsesStatusEndpoint(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId)
    {
        // arrange
        var order = new Order(isDelivery: true, totalAmount: 10m, notes: string.Empty, customerId: 1, createdAt: NodaTimeExtensions.NowUtc());
        var dto = new UpdateStatusDto { OrderId = orderId, Status = OrderStatusEnum.Cancelado };
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns(order);

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.UpdateStatusAsync(dto));

        // assert
        Assert.Equal("Cancelamento e erro não fazem parte do avanço normal do pedido.", exception.Message);
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task CancelAsync_ShouldThrowAndNotPersist_WhenStatusCannotBeCanceled(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int pedidoId)
    {
        // arrange: pedido em status que NÃO pode ser cancelado (ex.: Pronto)
        var order = new Order(isDelivery: true, totalAmount: 10m, notes: "obs", customerId: 1, createdAt: NodaTimeExtensions.NowUtc());
        order.AdvanceStatus();
        order.AdvanceStatus();
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(pedidoId)).Returns(order);

        // act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.CancelAsync(pedidoId));

        // assert
        var esperado = $"Pedido não pode ser cancelado no status atual: {order.Status}.";
        Assert.Equal(esperado, ex.Message);
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task CancelAsync_ShouldThrowAndNotWriteOrPublish_WhenOrderDoesNotExist(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId)
    {
        // arrange
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns((Order?)null);

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => orderService.CancelAsync(orderId));

        // assert
        Assert.Equal("Pedido não encontrado.", exception.Message);
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task CancelAsync_ShouldCancelPersistAndPublish_WhenStatusCanBeCanceled(
        [Frozen] IMessagePublisher messagePublisher,
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId,
        int customerId)
    {
        // arrange: pedido em status que pode ser cancelado (ex.: EmPreparacao)
        var customer = new Customer("Nome", "48999999999", "Rua", "Bairro", "123", "Cidade", "Ponto de referência", 88000000);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(customer, customerId);

        var order = new Order(isDelivery: true, totalAmount: 10m, notes: "obs", customerId: customerId, createdAt: NodaTimeExtensions.NowUtc());
        typeof(Order).GetProperty("OrderId")!.SetValue(order, orderId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, customer);

        order.AdvanceStatus();
        A.CallTo(() => orderRepository.GetByIdWithDetailsAsync(orderId)).Returns(order);

        // act
        await orderService.CancelAsync(orderId);

        // assert
        Assert.Equal(OrderStatusEnum.Cancelado, order.Status);
        A.CallTo(() => orderRepository.UpdateAsync(A<Order>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => messagePublisher.PublishAsync(
                A<NotifyMessageDto>.That.Matches(message =>
                    message.OrderId == orderId &&
                    message.OrderStatus == OrderStatusEnum.Cancelado &&
                    message.CustomerName == customer.Name &&
                    message.Phone == customer.Phone),
                RabbitMqConstants.Exchanges.NotifyExchange,
                A<string?>._))
            .MustHaveHappenedOnceExactly();
    }
}
