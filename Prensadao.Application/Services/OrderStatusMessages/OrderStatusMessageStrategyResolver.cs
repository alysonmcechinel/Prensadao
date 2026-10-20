using System.Diagnostics.CodeAnalysis;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Services.OrderStatusMessages;

public class OrderStatusMessageStrategyResolver : IOrderStatusMessageStrategyResolver
{
    private readonly IReadOnlyDictionary<OrderStatusEnum, IOrderStatusMessageStrategy> _strategies;

    public OrderStatusMessageStrategyResolver(IEnumerable<IOrderStatusMessageStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.Status);
    }

    public bool TryResolve(
        OrderStatusEnum status,
        [NotNullWhen(true)] out IOrderStatusMessageStrategy? strategy)
        => _strategies.TryGetValue(status, out strategy);
}
