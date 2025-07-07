namespace Prensadao.Application.Publish;

public interface IBus
{
    Task Publish<T>(T message, string exchange = "", string? routingKey = "");
}
