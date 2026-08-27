using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderFinishedNotificationStrategy : IOrderStatusNotificationStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Finalizado;

    public string BuildMessage(NotifyMessageDto message)
        => $"O seu pedido N°{message.OrderId} foi concluído. Agradecemos a preferência e bom apetite!";
}
