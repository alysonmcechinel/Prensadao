using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Domain;

public class OrderTest
{
    [Fact]
    public void ChangeStatus_ShouldFollowDeliveryFlow()
    {
        // arrange
        var order = CreateOrder(isDelivery: true);

        // act
        order.ChangeStatus(OrderStatusEnum.EmPreparacao);
        order.ChangeStatus(OrderStatusEnum.Pronto);
        order.ChangeStatus(OrderStatusEnum.SaiuParaEntrega);
        order.ChangeStatus(OrderStatusEnum.Finalizado);

        // assert
        Assert.Equal(OrderStatusEnum.Finalizado, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldFinishPickupAfterReady()
    {
        // arrange
        var order = CreateOrder(isDelivery: false);
        order.ChangeStatus(OrderStatusEnum.EmPreparacao);
        order.ChangeStatus(OrderStatusEnum.Pronto);

        // act
        order.ChangeStatus(OrderStatusEnum.Finalizado);

        // assert
        Assert.Equal(OrderStatusEnum.Finalizado, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenSkippingStatus()
    {
        // arrange
        var order = CreateOrder(isDelivery: true);

        // act
        var exception = Assert.Throws<ArgumentException>(() => order.ChangeStatus(OrderStatusEnum.Pronto));

        // assert
        Assert.Equal("Transição de status inválida: Criado para Pronto.", exception.Message);
        Assert.Equal(OrderStatusEnum.Criado, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldThrow_WhenStatusIsCanceled()
    {
        // arrange
        var order = CreateOrder(isDelivery: true);

        // act
        var exception = Assert.Throws<ArgumentException>(() => order.ChangeStatus(OrderStatusEnum.Cancelado));

        // assert
        Assert.Equal("Cancelamento não faz parte do avanço normal do pedido.", exception.Message);
        Assert.Equal(OrderStatusEnum.Criado, order.Status);
    }

    [Fact]
    public void ChangeStatus_ShouldDoNothing_WhenStatusIsAlreadySet()
    {
        // arrange
        var order = CreateOrder(isDelivery: true);

        // act
        order.ChangeStatus(OrderStatusEnum.Criado);

        // assert
        Assert.Equal(OrderStatusEnum.Criado, order.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Cancel_ShouldCancel_WhenStatusAllowsCancellation(int advances)
    {
        // arrange
        var order = CreateOrder(isDelivery: true);
        for (var index = 0; index < advances; index++)
            order.AdvanceStatus();

        // act
        order.Cancel();

        // assert
        Assert.Equal(OrderStatusEnum.Cancelado, order.Status);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Cancel_ShouldThrow_WhenStatusDoesNotAllowCancellation(int advances)
    {
        // arrange
        var order = CreateOrder(isDelivery: true);
        for (var index = 0; index < advances; index++)
            order.AdvanceStatus();

        var originalStatus = order.Status;

        // act
        var exception = Assert.Throws<ArgumentException>(() => order.Cancel());

        // assert
        Assert.Equal($"Pedido não pode ser cancelado no status atual: {originalStatus}.", exception.Message);
        Assert.Equal(originalStatus, order.Status);
    }

    [Fact]
    public void AdvanceStatus_ShouldThrow_WhenOrderIsFinalized()
    {
        // arrange
        var order = CreateOrder(isDelivery: false);
        order.AdvanceStatus();
        order.AdvanceStatus();
        order.AdvanceStatus();

        // act
        var exception = Assert.Throws<ArgumentException>(() => order.AdvanceStatus());

        // assert
        Assert.Equal("Pedido no status Finalizado não pode avançar.", exception.Message);
    }

    private static Order CreateOrder(bool isDelivery)
        => new(isDelivery, 10m, string.Empty, 1, DateTime.UtcNow);
}
