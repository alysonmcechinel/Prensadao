namespace Prensadao.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string exchange = "", string? routingKey = "");
}
