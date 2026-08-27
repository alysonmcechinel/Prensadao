using Prensadao.Application.Interfaces;
using Prensadao.Application.Services.Notifications;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderStatusNotificationStrategyFactoryTest
{
    [Fact]
    public void GetStrategy_DeveRetornarEstrategiaCorrespondenteAoStatus()
    {
        var preparingStrategy = new OrderPreparingNotificationStrategy();
        var readyStrategy = new OrderReadyNotificationStrategy();
        var factory = CreateFactory(preparingStrategy, readyStrategy);

        var result = factory.GetStrategy(OrderStatusEnum.Pronto);

        Assert.Same(readyStrategy, result);
    }

    [Fact]
    public void GetStrategy_DeveRetornarEstrategiaPadrao_QuandoStatusNaoPossuiEstrategiaRegistrada()
    {
        var factory = CreateFactory(new OrderPreparingNotificationStrategy());

        var result = factory.GetStrategy(OrderStatusEnum.Criado);

        Assert.IsType<UnknownOrderStatusNotificationStrategy>(result);
    }

    private static OrderStatusNotificationStrategyFactory CreateFactory(params IOrderStatusNotificationStrategy[] strategies)
        => new(strategies);
}
