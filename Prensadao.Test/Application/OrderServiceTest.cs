using AutoFixture.Xunit2;
using Castle.Core.Resource;
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

namespace Prensadao.Test.Application;

public class OrderServiceTest
{
    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_PedidoNaoPodeSerNulo(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService)
    {
        // Arrange
        OrderRequestDto? dto = null;

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreateAsync(dto!));

        // Assert
        Assert.Equal("O pedido não pode ser nulo.", ex.Message);
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
        dto.OrderItems = new List<OrderItemRequestDto>(); // vazio

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
            new(){ ProductId = 0, Quantity = 1 }
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
            new(){ ProductId = 10, Quantity = 0 }
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
            new(){ ProductId = 10, Quantity = 1 }
        };

        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._)).Returns(true); // força caminho de produto inativo

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
        A.CallTo(() => orderRepository.GetById(pedidoId)).Returns(order);

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
        A.CallTo(() => orderRepository.GetById(orderId)).Returns(order);

        // Act
        await orderService.EnabledAsync(orderId);

        // Assert
        Assert.Equal(OrderStatusEnum.Cancelado, order.OrderStatus);
        A.CallTo(() => orderRepository.Update(A<Order>._)).MustHaveHappenedOnceExactly();
    }
}
