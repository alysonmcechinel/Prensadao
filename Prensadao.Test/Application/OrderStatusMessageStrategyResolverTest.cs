using Prensadao.Application.Interfaces;
using Prensadao.Application.Services.OrderStatusMessages;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusMessageStrategyResolverTest
{
    [Fact]
    public void TryResolve_ShouldReturnMatchingStrategy_WhenStatusProducesMessage()
    {
        // arrange
        var preparingStrategy = new OrderPreparingMessageStrategy();
        var readyStrategy = new OrderReadyMessageStrategy();
        var resolver = CreateResolver(preparingStrategy, readyStrategy);

        // act
        var found = resolver.TryResolve(OrderStatusEnum.Pronto, out var result);

        // assert
        Assert.True(found);
        Assert.Same(readyStrategy, result);
    }

    [Fact]
    public void TryResolve_ShouldReturnFalse_WhenValidStatusDoesNotProduceMessage()
    {
        // arrange
        var resolver = CreateResolver(new OrderPreparingMessageStrategy());

        // act
        var found = resolver.TryResolve(OrderStatusEnum.Criado, out var result);

        // assert
        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void TryResolve_ShouldReturnFalse_WhenStatusValueIsInvalid()
    {
        // arrange
        var resolver = CreateResolver(new OrderPreparingMessageStrategy());

        // act
        var found = resolver.TryResolve((OrderStatusEnum)999, out var result);

        // assert
        Assert.False(found);
        Assert.Null(result);
    }

    private static OrderStatusMessageStrategyResolver CreateResolver(params IOrderStatusMessageStrategy[] strategies)
        => new(strategies);
}
