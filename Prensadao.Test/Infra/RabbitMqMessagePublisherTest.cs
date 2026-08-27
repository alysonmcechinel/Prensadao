using FakeItEasy;
using Prensadao.Application.DTOs;
using Prensadao.Infra.Messaging.Interfaces;
using Prensadao.Infra.Messaging.RabbitMq;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prensadao.Test.Infra;

public class RabbitMqMessagePublisherTest
{
    [Fact]
    public async Task PublishAsync_DevePublicarMensagemPersistenteEmJson_QuandoMensagemValida()
    {
        // Arrange
        var rabbitMqConfig = A.Fake<IRabbitMqConfig>();
        var channel = A.Fake<IModel>();
        var properties = A.Fake<IBasicProperties>();
        var publisher = new RabbitMqMessagePublisher(rabbitMqConfig);

        var message = new OrderMessageDto
        {
            OrderId = 123
        };

        const string exchange = "order.exchange";
        const string routingKey = "order.created";
        var expectedJson = JsonSerializer.Serialize(message);

        A.CallTo(() => rabbitMqConfig.CreateChannel()).Returns(channel);
        A.CallTo(() => channel.CreateBasicProperties()).Returns(properties);

        // Act
        await publisher.PublishAsync(message, exchange, routingKey);

        // Assert
        A.CallTo(() => rabbitMqConfig.CreateChannel())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => channel.CreateBasicProperties())
            .MustHaveHappenedOnceExactly();

        A.CallToSet(() => properties.Persistent)
            .To(true)
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => channel.BasicPublish(
                exchange,
                routingKey,
                false,
                properties,
                A<ReadOnlyMemory<byte>>.That.Matches(body => Encoding.UTF8.GetString(body.ToArray()) == expectedJson)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task PublishAsync_DeveLancarArgumentNullException_QuandoExchangeVazio()
    {
        // Arrange
        var publisher = new RabbitMqMessagePublisher(A.Fake<IRabbitMqConfig>());
        var message = new OrderMessageDto { OrderId = 123 };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => publisher.PublishAsync(message, ""));

        // Assert
        Assert.Equal("exchange", exception.ParamName);
    }

    [Fact]
    public async Task PublishAsync_DeveLancarArgumentNullException_QuandoMensagemNula()
    {
        // Arrange
        var publisher = new RabbitMqMessagePublisher(A.Fake<IRabbitMqConfig>());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => publisher.PublishAsync<OrderMessageDto>(null!, "order.exchange"));

        // Assert
        Assert.Equal("message", exception.ParamName);
    }
}
