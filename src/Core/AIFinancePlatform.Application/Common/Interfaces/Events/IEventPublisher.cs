using System.Threading.Tasks;

namespace AIFinancePlatform.Application.Common.Interfaces.Events;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string queueName) where T : class;
}
