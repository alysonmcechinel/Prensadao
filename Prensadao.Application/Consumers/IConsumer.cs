namespace Prensadao.Application.Consumers;

public interface IConsumer
{
    Task Listen<T>(string queue, Func<T, Task> onMessage);
}
