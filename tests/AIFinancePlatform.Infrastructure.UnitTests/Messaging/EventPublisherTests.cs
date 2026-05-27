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
    private readonly Mock<IModel> _mockModel;
    private readonly RabbitMqEventPublisher _publisher;

    public EventPublisherTests()
    {
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockConnection = new Mock<IConnection>();
        _mockModel = new Mock<IModel>();

        _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(c => c.CreateModel()).Returns(_mockModel.Object);

        _publisher = new RabbitMqEventPublisher(_mockConnectionFactory.Object);
    }

    [Fact]
    public void Publish_ShouldSerializeAndSendToRabbitMQ()
    {
        // Arrange
        var testEvent = new { ReceiptId = 123, UserId = "user-1" };
        var exchange = "test_exchange";
        var routingKey = "test_routing_key";

        // Act
        _publisher.Publish(testEvent, exchange, routingKey);

        // Assert
        _mockModel.Verify(m => m.ExchangeDeclare(exchange, ExchangeType.Direct, true, false, null), Times.Once);
        
        _mockModel.Verify(m => m.BasicPublish(
            exchange,
            routingKey,
            It.IsAny<bool>(),
            It.IsAny<IBasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
    }
}
