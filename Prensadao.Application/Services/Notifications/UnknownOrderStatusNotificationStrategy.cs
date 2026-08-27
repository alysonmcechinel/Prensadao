using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class UnknownOrderStatusNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Error;

    public string BuildMessage(NotifyMessageDto message)
        => $"O seu pedido N°{message.OrderId} está com status não identificado.";
}
