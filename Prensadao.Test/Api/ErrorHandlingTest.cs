using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Transactions;
using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prensadao.API.Controllers;
using Prensadao.API.ErrorHandling;
using Prensadao.API.Extensions;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.Exceptions;
using Prensadao.Application.Interfaces;

namespace Prensadao.Test.Api;

public class ErrorHandlingTest
{
    public static TheoryData<Exception, int> Failures => new()
    {
        { new RequestValidationException("Quantidade inválida."), 400 },
        { new ResourceNotFoundException("Pedido não encontrado."), 404 },
        { new ConflictException("Transição inválida."), 409 },
        { new AiResponseValidationException("Resposta da IA inválida."), 422 },
        { new InsufficientSalesDataException("Vendas insuficientes."), 422 },
        { new TransactionAbortedException("transaction-private-detail", new InvalidOperationException("database-inner-detail")), 500 },
        { new ArgumentException("unexpected-private-detail"), 500 },
        { new InvalidOperationException("unexpected-private-detail"), 500 }
    };

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task Pipeline_ShouldReturnProblemAndLogExceptionOnce_WhenActionFails(
        Exception exception, int expectedStatus)
    {
        // arrange
        var orderService = A.Fake<IOrderService>();
        A.CallTo(() => orderService.OrderCreateAsync(A<OrderRequestDto>._)).ThrowsAsync(exception);
        var recorder = new RecordingLoggerProvider();
        await using var app = await CreateApplicationAsync(orderService, recorder);
        using var client = app.GetTestClient();

        // act
        using var response = await client.PostAsJsonAsync("/api/Order/Post", ValidOrder());
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;

        // assert
        Assert.Equal(expectedStatus, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expectedStatus, problem.GetProperty("status").GetInt32());
        Assert.Equal("/api/Order/Post", problem.GetProperty("instance").GetString());
        var traceId = problem.GetProperty("traceId").GetString();
        Assert.False(string.IsNullOrWhiteSpace(traceId));
        var log = Assert.Single(recorder.Entries.Where(entry => entry.Exception is not null));
        Assert.Equal(typeof(GlobalExceptionHandler).FullName, log.Category);
        Assert.Same(exception, log.Exception);
        Assert.Equal(traceId, log.Properties["TraceId"]);
        Assert.Equal(expectedStatus >= 500 ? LogLevel.Error : LogLevel.Warning, log.Level);
        if (expectedStatus >= 500)
        {
            Assert.DoesNotContain("private-detail", body);
            Assert.DoesNotContain("inner-detail", body);
        }
    }

    [Theory]
    [InlineData("{\"customerId\":1,\"orderItems\":[]}")]
    [InlineData("{\"customerId\":\"invalid\",\"isDelivery\":false}")]
    [InlineData("{invalid-json")]
    [InlineData("null")]
    public async Task Pipeline_ShouldReturnValidationProblemWithoutCallingService_WhenModelBindingFails(string json)
    {
        // arrange
        var service = A.Fake<IOrderService>();
        await using var app = await CreateApplicationAsync(service, new RecordingLoggerProvider());
        using var client = app.GetTestClient();
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // act
        using var response = await client.PostAsync("/api/Order/Post", content);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(document.RootElement.TryGetProperty("errors", out _));
        A.CallTo(() => service.OrderCreateAsync(A<OrderRequestDto>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Pipeline_ShouldPreserveSuccessContract_WhenOrderIsCreated()
    {
        // arrange
        var service = A.Fake<IOrderService>();
        A.CallTo(() => service.OrderCreateAsync(A<OrderRequestDto>._)).Returns(123);
        await using var app = await CreateApplicationAsync(service, new RecordingLoggerProvider());
        using var client = app.GetTestClient();

        // act
        using var response = await client.PostAsJsonAsync("/api/Order/Post", ValidOrder());
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(123, document.RootElement.GetProperty("data").GetInt32());
        Assert.Equal("Pedido criado com sucesso.", document.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Pipeline_ShouldKeepProblemContract_WhenAcceptIsUnsupported()
    {
        // arrange
        var service = A.Fake<IOrderService>();
        A.CallTo(() => service.OrderCreateAsync(A<OrderRequestDto>._)).ThrowsAsync(new Exception("private-detail"));
        await using var app = await CreateApplicationAsync(service, new RecordingLoggerProvider());
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Accept.ParseAdd("text/plain");

        // act
        using var response = await client.PostAsJsonAsync("/api/Order/Post", ValidOrder());

        // assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("private-detail", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/Customer/GetById?id=1")]
    [InlineData("/api/Product/GetById?id=1")]
    public async Task Pipeline_ShouldHandleMissingResource_WhenOtherControllerFails(string path)
    {
        // arrange
        await using var app = await CreateApplicationAsync(A.Fake<IOrderService>(), new RecordingLoggerProvider());
        using var client = app.GetTestClient();

        // act
        using var response = await client.GetAsync(path);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Pipeline_ShouldReturnUnprocessableEntity_WhenAiResponseIsInvalid()
    {
        // arrange
        var failure = new AiResponseValidationException("Resposta inválida da IA.");
        await using var app = await CreateApplicationAsync(A.Fake<IOrderService>(), new RecordingLoggerProvider(), failure);
        using var client = app.GetTestClient();

        // act
        using var response = await client.PostAsJsonAsync("/api/Promotions/Generate",
            new GeneratePromotionRequest { TopN = 5, MinDiscountPercent = 5, MaxDiscountPercent = 25 });

        // assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private static OrderRequestDto ValidOrder() => new()
    {
        IsDelivery = false,
        CustomerId = 1,
        OrderItems = [new() { ProductId = 1, Quantity = 1 }]
    };

    private static async Task<WebApplication> CreateApplicationAsync(
        IOrderService orderService, RecordingLoggerProvider recorder, Exception? promotionFailure = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(recorder);
        builder.Services.AddApiErrorHandling();
        builder.Services.AddControllers().AddApplicationPart(typeof(OrderController).Assembly);
        builder.Services.AddSingleton(orderService);
        var customerService = A.Fake<ICustomerService>();
        A.CallTo(() => customerService.GetByIdAsync(A<int>._)).ThrowsAsync(new ResourceNotFoundException("Cliente não encontrado."));
        builder.Services.AddSingleton(customerService);
        var productService = A.Fake<IProductService>();
        A.CallTo(() => productService.GetByIdAsync(A<int>._)).ThrowsAsync(new ResourceNotFoundException("Produto não encontrado."));
        builder.Services.AddSingleton(productService);
        var promotionGenerator = A.Fake<IPromotionGenerator>();
        if (promotionFailure is not null)
            A.CallTo(() => promotionGenerator.GeneratePromotionAsync(A<GeneratePromotionRequest>._, A<CancellationToken>._))
                .ThrowsAsync(promotionFailure);
        builder.Services.AddSingleton(promotionGenerator);
        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private sealed record LogEntry(string Category, LogLevel Level, Exception? Exception, Dictionary<string, object?> Properties);

    private sealed class RecordingLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();
        public ILogger CreateLogger(string categoryName) => new RecordingLogger(categoryName, Entries);
        public void Dispose() { }

        private sealed class RecordingLogger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                    ? values.ToDictionary(value => value.Key, value => value.Value)
                    : new Dictionary<string, object?>();
                entries.Enqueue(new LogEntry(category, logLevel, exception, properties));
            }
        }
    }
}
