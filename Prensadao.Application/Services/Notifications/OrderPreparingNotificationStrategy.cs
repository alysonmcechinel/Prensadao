using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderPreparingNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.EmPreparacao;

    public string BuildMessage(NotifyMessageDto message)
        => $"O seu pedido N°{message.OrderId} está em preparação.";
}
