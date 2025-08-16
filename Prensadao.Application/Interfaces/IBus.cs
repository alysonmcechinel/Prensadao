namespace Prensadao.Application.Interfaces;

public interface IBus
{
    Task Publish<T>(T message, string exchange = "", string? routingKey = "");
}
