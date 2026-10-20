using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderCanceledMessageStrategy : IOrderStatusMessageStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.Cancelado;

    public string BuildMessage(OrderStatusChangedMessageDto message)
        => $"O seu pedido N°{message.OrderId} foi cancelado.";
}
