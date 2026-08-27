using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.Notifications;

public class OrderStatusNotificationStrategyFactory : IOrderStatusNotificationStrategyFactory
{
    private readonly IReadOnlyDictionary<OrderStatusEnum, IOrderStatusNotificationStrategy> _strategies;
    private readonly UnknownOrderStatusNotificationStrategy _unknownStrategy = new();

    public OrderStatusNotificationStrategyFactory(IEnumerable<IOrderStatusNotificationStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.Status);
    }

    public IOrderStatusNotificationStrategy GetStrategy(OrderStatusEnum status)
    {
        if (_strategies.TryGetValue(status, out var strategy))
            return strategy;

        return _unknownStrategy;
    }
}
