using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderReadyMessageStrategy : IOrderStatusMessageStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Pronto;

    public string BuildMessage(OrderStatusChangedMessageDto message)
    {
        if (message.IsDelivery)
            return $"O seu pedido N°{message.OrderId} está pronto e aguardando saída para entrega.";

        return $"O seu pedido N°{message.OrderId} está pronto para retirada.";
    }
}
