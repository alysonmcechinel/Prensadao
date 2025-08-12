using AutoFixture.Xunit2;
using FakeItEasy;
using Prensadao.Application.Helpers;
using Prensadao.Application.Models.Request;
using Prensadao.Application.Publish;
using Prensadao.Application.Services;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enum;
using Prensadao.Domain.Repositories;
using System.Reflection;

namespace Prensadao.Test.Application;

public class OrderServiceTest
{
    [Theory, AutoFakeItEasyData]
    public async Task OrderCreate_DeveLancar_PedidoNaoPodeSerNulo(
        [Frozen] IOrderRepository orderRepository,
        OrderService orderService,
        OrderRequestDto dto)
    {
        // Arrange
        dto = null;

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

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
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

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
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

        // Assert
        Assert.Equal("Pedido não pode ser feito sem items.", ex.Message);
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
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

        // Assert
        Assert.Equal("Pedido contém Id inválido.", ex.Message);
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
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

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

        A.CallTo(() => productRepository.ExistsInactiveProduct(A<List<int>>._))
            .Returns(true); // força caminho de produto inativo

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.OrderCreate(dto));

        // Assert
        Assert.Equal("Pedido não pode ser feito com produtos inativos.", ex.Message);
        A.CallTo(() => orderRepository.CreateOrder(A<Order>._)).MustNotHaveHappened();
    }

    [Theory, AutoFakeItEasyData]
    public async Task Enabled_DeveLancar_JaEstaComStatusNaoCancelavel(
       [Frozen] IOrderRepository orderRepository,
       OrderService orderService)
    {
        // Arrange: pedido em status que NÃO pode ser cancelado (ex.: Pronto)
        var order = new Order(delivery: true, value: 10m, observation: "obs", customerId: 1);
        order.UpdateStatus(OrderStatusEnum.Pronto);

        A.CallTo(() => orderRepository.GetById(123))
            .Returns(order);

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => orderService.Enabled(123));

        // Assert
        var esperado = $"Pedido não pode ser cancelado pois, já esta com status: {order.OrderStatus.GetDescription()}";
        Assert.Equal(esperado, ex.Message);

        A.CallTo(() => orderRepository.Update(A<Order>._)).MustNotHaveHappened();
    }
}
