using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderCanceledNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Cancelado;

    public string BuildMessage(NotifyMessageDto message)
        => $"O seu pedido N°{message.OrderId} foi cancelado.";
}
