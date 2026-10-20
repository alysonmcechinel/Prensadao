using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderPreparingMessageStrategy : IOrderStatusMessageStrategy
{
    public OrderStatusEnum Status => OrderStatusEnum.EmPreparacao;

    public string BuildMessage(OrderStatusChangedMessageDto message)
        => $"O seu pedido N°{message.OrderId} está em preparação.";
}
