using Prensadao.Application.DTOs;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Interfaces;

public interface IOrderStatusMessageStrategy
{
    OrderStatusEnum Status { get; }

    string BuildMessage(OrderStatusChangedMessageDto message);
}
