using Prensadao.Application.DTOs;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Interfaces;

public interface IOrderStatusNotificationStrategy
{
    OrderStatusEnum Status { get; }

    string BuildMessage(NotifyMessageDto message);
}
