using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderOutForDeliveryNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.SaiuParaEntrega;

    public string BuildMessage(NotifyMessageDto message)
        => $"O seu pedido N°{message.OrderId} saiu para entrega.";
}
