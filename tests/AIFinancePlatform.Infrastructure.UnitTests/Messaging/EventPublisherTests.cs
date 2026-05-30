using System;
using System.Text;
using System.Text.Json;
using AIFinancePlatform.Application.Common.Interfaces.Events;
using AIFinancePlatform.Infrastructure.Messaging;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace AIFinancePlatform.Infrastructure.UnitTests.Messaging;

public class EventPublisherTests
{
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IChannel> _mockChannel;
    private readonly RabbitMqEventPublisher _publisher;

    public EventPublisherTests()
    {
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockConnection = new Mock<IConnection>();
        _mockChannel = new Mock<IChannel>();

        _mockConnectionFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                              .ReturnsAsync(_mockConnection.Object);
                              
        _mockConnection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(_mockChannel.Object);

        _publisher = new RabbitMqEventPublisher(_mockConnectionFactory.Object);
    }

    [Fact]
    public async Task PublishAsync_ShouldSerializeAndSendToRabbitMQ()
    {
        // Arrange
        var testEvent = new { ReceiptId = 123, UserId = "user-1" };
        var queueName = "test_queue";

        // Act
        await _publisher.PublishAsync(testEvent, queueName);

        // Assert
        _mockChannel.Verify(m => m.QueueDeclareAsync(
            queueName,
            true,
            false,
            false,
            null,
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
            
        _mockChannel.Verify(m => m.BasicPublishAsync(
            string.Empty,
            queueName,
            false,
            It.IsAny<BasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
