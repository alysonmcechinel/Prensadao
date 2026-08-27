using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderReadyNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Pronto;

    public string BuildMessage(NotifyMessageDto message)
    {
        if (message.Delivery)
            return $"O seu pedido N°{message.OrderId} está pronto e aguardando saída para entrega.";

        return $"O seu pedido N°{message.OrderId} está pronto para retirada.";
    }
}
