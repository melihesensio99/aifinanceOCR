 using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using AIFinancePlatform.Application.Common.Interfaces.Events;

namespace AIFinancePlatform.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqEventPublisher()
    {
        _factory = new ConnectionFactory { HostName = "localhost" };
    }

    public async Task PublishAsync<T>(T @event, string queueName) where T : class
    {
        using var connection = await _factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            body: body,
            mandatory: false);
    }
}
