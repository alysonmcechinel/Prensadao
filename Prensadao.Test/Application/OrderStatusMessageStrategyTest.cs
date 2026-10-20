using Prensadao.Application.DTOs;
using Prensadao.Application.Services.OrderStatusMessages;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusMessageStrategyTest
{
    [Fact]
    public void BuildMessage_ShouldReturnPreparingMessage()
    {
        // arrange
        var strategy = new OrderPreparingMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.EmPreparacao);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está em preparação.", result);
    }

    [Fact]
    public void BuildMessage_ShouldReturnReadyForDeliveryMessage()
    {
        // arrange
        var strategy = new OrderReadyMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, isDelivery: true);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está pronto e aguardando saída para entrega.", result);
    }

    [Fact]
    public void BuildMessage_ShouldReturnReadyForPickupMessage()
    {
        // arrange
        var strategy = new OrderReadyMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, isDelivery: false);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está pronto para retirada.", result);
    }

    [Fact]
    public void BuildMessage_ShouldReturnOutForDeliveryMessage()
    {
        // arrange
        var strategy = new OrderOutForDeliveryMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.SaiuParaEntrega);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 saiu para entrega.", result);
    }

    [Fact]
    public void BuildMessage_ShouldReturnCanceledMessage()
    {
        // arrange
        var strategy = new OrderCanceledMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.Cancelado);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 foi cancelado.", result);
    }

    [Fact]
    public void BuildMessage_ShouldReturnFinishedMessage()
    {
        // arrange
        var strategy = new OrderFinishedMessageStrategy();
        var message = CreateMessage(OrderStatusEnum.Finalizado);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 foi concluído. Agradecemos a preferência e bom apetite!", result);
    }

    private static OrderStatusChangedMessageDto CreateMessage(OrderStatusEnum status, bool isDelivery = true)
        => new()
        {
            OrderId = 10,
            Status = status,
            IsDelivery = isDelivery
        };
}
