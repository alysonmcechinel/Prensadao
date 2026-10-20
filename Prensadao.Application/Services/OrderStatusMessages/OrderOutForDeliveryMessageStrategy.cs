using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderOutForDeliveryMessageStrategy : IOrderStatusMessageStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.SaiuParaEntrega;

    public string BuildMessage(OrderStatusChangedMessageDto message)
        => $"O seu pedido N°{message.OrderId} saiu para entrega.";
}
