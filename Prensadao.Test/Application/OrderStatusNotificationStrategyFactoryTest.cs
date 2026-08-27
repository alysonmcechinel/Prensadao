using Prensadao.Application.Interfaces;
using Prensadao.Application.Services.Notifications;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusNotificationStrategyFactoryTest
{
    [Fact]
    public void GetStrategy_DeveRetornarEstrategiaCorrespondenteAoStatus()
    {
        // arrange
        var preparingStrategy = new OrderPreparingNotificationStrategy();
        var readyStrategy = new OrderReadyNotificationStrategy();
        var factory = CreateFactory(preparingStrategy, readyStrategy);

        // act
        var result = factory.GetStrategy(OrderStatusEnum.Pronto);

        // assert
        Assert.Same(readyStrategy, result);
    }

    [Fact]
    public void GetStrategy_DeveRetornarEstrategiaPadrao_QuandoStatusNaoPossuiEstrategiaRegistrada()
    {
        // arrange
        var factory = CreateFactory(new OrderPreparingNotificationStrategy());

        // act
        var result = factory.GetStrategy(OrderStatusEnum.Criado);

        // assert
        Assert.IsType<UnknownOrderStatusNotificationStrategy>(result);
    }

    private static OrderStatusNotificationStrategyFactory CreateFactory(params IOrderStatusNotificationStrategy[] strategies)
        => new(strategies);
}
