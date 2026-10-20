using System.Diagnostics.CodeAnalysis;
using Prensadao.Domain.Enums;

namespace Prensadao.Application.Interfaces;

public interface IOrderStatusMessageStrategyResolver
{
    bool TryResolve(OrderStatusEnum status, [NotNullWhen(true)] out IOrderStatusMessageStrategy? strategy);
}
