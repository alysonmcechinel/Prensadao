using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderFinishedMessageStrategy : IOrderStatusMessageStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Finalizado;

    public string BuildMessage(OrderStatusChangedMessageDto message)
        => $"O seu pedido N°{message.OrderId} foi concluído. Agradecemos a preferência e bom apetite!";
}
