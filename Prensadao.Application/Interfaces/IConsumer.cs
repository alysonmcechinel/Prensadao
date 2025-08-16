namespace Prensadao.Application.Interfaces;

public interface IConsumer
{
    Task Listen<T>(string queue, Func<T, Task> onMessage);
}
