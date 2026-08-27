using Prensadao.Application.DTOs;
using Prensadao.Application.Services.Notifications;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusNotificationStrategyTest
{
    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoEmPreparacao()
    {
        // arrange
        var strategy = new OrderPreparingNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.EmPreparacao);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está em preparação.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoProntoParaEntrega()
    {
        // arrange
        var strategy = new OrderReadyNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, delivery: true);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está pronto e aguardando saída para entrega.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoProntoParaRetirada()
    {
        // arrange
        var strategy = new OrderReadyNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, delivery: false);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está pronto para retirada.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoSaiuParaEntrega()
    {
        // arrange
        var strategy = new OrderOutForDeliveryNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.SaiuParaEntrega);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 saiu para entrega.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoCancelado()
    {
        // arrange
        var strategy = new OrderCanceledNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Cancelado);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 foi cancelado.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoFinalizado()
    {
        // arrange
        var strategy = new OrderFinishedNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Finalizado);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 foi concluído. Agradecemos a preferência e bom apetite!", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDeStatusNaoIdentificado()
    {
        // arrange
        var strategy = new UnknownOrderStatusNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Error);

        // act
        var result = strategy.BuildMessage(message);

        // assert
        Assert.Equal("O seu pedido N°10 está com status não identificado.", result);
    }

    private static NotifyMessageDto CreateMessage(OrderStatusEnum status, bool delivery = true)
        => new()
        {
            OrderId = 10,
            OrderStatus = status,
            Delivery = delivery
        };
}
