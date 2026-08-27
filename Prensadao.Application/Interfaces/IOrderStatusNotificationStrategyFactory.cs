using Prensadao.Domain.Enums;

namespace Prensadao.Application.Interfaces;

public interface IOrderStatusNotificationStrategyFactory
{
    IOrderStatusNotificationStrategy GetStrategy(OrderStatusEnum status);
}
