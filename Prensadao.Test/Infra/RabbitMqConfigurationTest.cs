using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prensadao.Infra;
using Prensadao.Infra.Messaging.RabbitMq;
using RabbitMQ.Client;

namespace Prensadao.Test.Infra;

public class RabbitMqConfigurationTest
{
    [Fact]
    public void AddRabbitMQ_ShouldBindConnectionFactory_WhenConfigurationIsValid()
    {
        // arrange
        var configuration = CreateConfiguration(
            host: "rabbitmq",
            port: "5673",
            user: "app-user",
            password: "app-password");
        var services = new ServiceCollection();

        // act
        services.AddRabbitMQ(configuration);
        using var provider = services.BuildServiceProvider();
        var result = provider.GetRequiredService<IConnectionFactory>();

        // assert
        result.Should().BeOfType<ConnectionFactory>();
        var factory = (ConnectionFactory)result;
        factory.HostName.Should().Be("rabbitmq");
        factory.Port.Should().Be(5673);
        factory.UserName.Should().Be("app-user");
        factory.Password.Should().Be("app-password");
        factory.DispatchConsumersAsync.Should().BeTrue();
    }

    [Theory]
    [InlineData("RabbitMq:Host", "")]
    [InlineData("RabbitMq:Port", "0")]
    [InlineData("RabbitMq:Port", "65536")]
    [InlineData("RabbitMq:User", "")]
    [InlineData("RabbitMq:Password", "")]
    public void AddRabbitMQ_ShouldRejectInvalidConfiguration_WhenRequiredValueIsInvalid(
        string invalidKey,
        string invalidValue)
    {
        // arrange
        var values = ValidConfigurationValues();
        values[invalidKey] = invalidValue;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();

        // act
        services.AddRabbitMQ(configuration);
        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<IConnectionFactory>();

        // assert
        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public void Constructor_ShouldCreateNamedConnection_WhenFactoryIsProvided()
    {
        // arrange
        var connectionFactory = A.Fake<IConnectionFactory>();
        var connection = A.Fake<IConnection>();
        A.CallTo(() => connectionFactory.CreateConnection("prensadao-app"))
            .Returns(connection);

        // act
        using var result = new RabbitMqConfig(connectionFactory);

        // assert
        A.CallTo(() => connectionFactory.CreateConnection("prensadao-app"))
            .MustHaveHappenedOnceExactly();
    }

    private static IConfiguration CreateConfiguration(string host, string port, string user, string password)
    {
        var values = ValidConfigurationValues();
        values["RabbitMq:Host"] = host;
        values["RabbitMq:Port"] = port;
        values["RabbitMq:User"] = user;
        values["RabbitMq:Password"] = password;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static Dictionary<string, string?> ValidConfigurationValues()
        => new()
        {
            ["RabbitMq:Host"] = "localhost",
            ["RabbitMq:Port"] = "5672",
            ["RabbitMq:User"] = "guest",
            ["RabbitMq:Password"] = "guest"
        };
}
