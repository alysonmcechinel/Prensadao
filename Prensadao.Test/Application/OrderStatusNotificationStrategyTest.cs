using Prensadao.Application.DTOs;
using Prensadao.Application.Services.Notifications;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusNotificationStrategyTest
{
    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoEmPreparacao()
    {
        var strategy = new OrderPreparingNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.EmPreparacao);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 está em preparação.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoProntoParaEntrega()
    {
        var strategy = new OrderReadyNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, delivery: true);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 está pronto e aguardando saída para entrega.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoProntoParaRetirada()
    {
        var strategy = new OrderReadyNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Pronto, delivery: false);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 está pronto para retirada.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoSaiuParaEntrega()
    {
        var strategy = new OrderOutForDeliveryNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.SaiuParaEntrega);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 saiu para entrega.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoCancelado()
    {
        var strategy = new OrderCanceledNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Cancelado);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 foi cancelado.", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDePedidoFinalizado()
    {
        var strategy = new OrderFinishedNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Finalizado);

        var result = strategy.BuildMessage(message);

        Assert.Equal("O seu pedido N°10 foi concluído. Agradecemos a preferência e bom apetite!", result);
    }

    [Fact]
    public void BuildMessage_DeveRetornarMensagemDeStatusNaoIdentificado()
    {
        var strategy = new UnknownOrderStatusNotificationStrategy();
        var message = CreateMessage(OrderStatusEnum.Error);

        var result = strategy.BuildMessage(message);

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
