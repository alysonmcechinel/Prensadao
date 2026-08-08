namespace Prensadao.Application.Interfaces;

public interface IPublish
{
    Task Publish<T>(T message, string exchange = "", string? routingKey = "");
}
