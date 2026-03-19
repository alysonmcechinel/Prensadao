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
        [Frozen] IBus bus,
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IOrderItemRepository orderItemRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService)
    {
        // Arrange
        var dto = new OrderRequestDto
        {
            CustomerId = 1,
            Delivery = true,
            Observation = "Sem cebola",
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

        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).Returns(false);
        A.CallTo(() => productRepository.ValueOfProducts(A<List<int>>.That.Matches(ids =>
            ids.Count == 2 &&
            ids.Contains(10) &&
            ids.Contains(20)))).Returns(products);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._))
            .Invokes((Order order) => typeof(Order).GetProperty("OrderId")!.SetValue(order, 123));

        // Act
        var result = await orderService.OrderCreateAsync(dto);

        // Assert
        Assert.Equal(123, result);

        A.CallTo(() => orderRepository.CreateOrder(
            A<Order>.That.Matches(order =>
                order.CustomerId == dto.CustomerId &&
                order.Delivery == dto.Delivery &&
                order.Observation == dto.Observation &&
                order.Value == 39.00m &&
                order.OrderStatus == OrderStatusEnum.Criado)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderItemRepository.AddOrderItemAsync(
            A<OrderItem>.That.Matches(item =>
                item.OrderId == 123 &&
                item.ProductId == 10 &&
                item.Quantity == 2 &&
                item.UnitPrice == 15.50m)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => orderItemRepository.AddOrderItemAsync(
            A<OrderItem>.That.Matches(item =>
                item.OrderId == 123 &&
                item.ProductId == 20 &&
                item.Quantity == 1 &&
                item.UnitPrice == 8.00m)))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => bus.Publish(
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
        // Arrange
        OrderRequestDto? dto = null;

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() => orderService.OrderCreateAsync(dto!));

        // Assert
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_PedidoSemClienteCadastrado(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto.CustomerId = 0;

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // Assert
        Assert.Equal("Pedido não pode ser feito sem cliente cadastrado.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_SemItens(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto.OrderItems = new List<OrderItemRequestDto>();

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // Assert
        Assert.Equal("Pedido não pode ser feito sem itens.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComProdutoIdInvalido(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 0, Quantity = 1 }
        };

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // Assert
        Assert.Equal("Pedido contém itens com ProductId inválido.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComQuantidadeInvalida(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 10, Quantity = 0 }
        };

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // Assert
        Assert.Equal("Pedido contém itens com quantidade inválida.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_ComProdutoInativo(
        [Frozen] IOrderRepository orderRepository,
        [Frozen] IProductRepository productRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto.OrderItems = new List<OrderItemRequestDto>
        {
            new() { ProductId = 10, Quantity = 1 }
        };

        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).Returns(true);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto));

        // Assert
        Assert.Equal("Pedido não pode ser feito com produtos inativos.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task Enabled_DeveLancar_JaEstaComStatusNaoCancelavel(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int pedidoId)
    {
        // Arrange: pedido em status que NÃO pode ser cancelado (ex.: Pronto)
        var order = new Order(delivery: true, value: 10m, observation: "obs", customerId: 1, NodaTimeExtensions.NowUtc());
        order.UpdateStatus(OrderStatusEnum.Pronto);
        A.CallTo(() => orderRepository.GetByIdAsync(pedidoId)).Returns(order);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.EnabledAsync(pedidoId));

        // Assert
        var esperado = $"Pedido não pode ser cancelado pois, já esta com status: {order.OrderStatus.GetDescription()}";
        Assert.Equal(esperado, ex.Message);
        A.CallTo(() => orderRepository.Update(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task Enabled_DeveCancelar_PedidoComStatusCancelavel(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        int orderId,
        int customerId)
    {
        // Arrange: pedido em status que pode ser cancelado (ex.: EmPreparacao)
        var custumer = new Customer("Nome", "48999999999", "Rua", "Bairro", "123", "Cidade", "Ponto de referência", 88000000);
        typeof(Customer).GetProperty("CustomerId")!.SetValue(custumer, customerId);

        var order = new Order(delivery: true, value: 10m, observation: "obs", customerId: customerId, NodaTimeExtensions.NowUtc());
        typeof(Order).GetProperty("OrderId")!.SetValue(order, orderId);
        typeof(Order).GetProperty("Customer")!.SetValue(order, custumer);

        order.UpdateStatus(OrderStatusEnum.EmPreparacao);
        A.CallTo(() => orderRepository.GetByIdAsync(orderId)).Returns(order);

        // Act
        await orderService.EnabledAsync(orderId);

        // Assert
        Assert.Equal(OrderStatusEnum.Cancelado, order.OrderStatus);
        A.CallTo(() => orderRepository.Update(A<Order>._)).MustHaveHappenedOnceExactly();
    }
}
