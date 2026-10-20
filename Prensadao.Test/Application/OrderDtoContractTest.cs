using System.Text.Json;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderDtoContractTest
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void OrderResponseDto_ShouldSerializeCanonicalFields()
    {
        // arrange
        var createdAtUtc = new DateTime(2026, 10, 13, 18, 0, 0, DateTimeKind.Utc);
        var dto = new OrderResponseDto(
            1,
            OrderStatusEnum.Criado,
            true,
            42.50m,
            "Sem cebola",
            10,
            "Cliente",
            createdAtUtc,
            []);

        // act
        var json = JsonSerializer.Serialize(dto, WebJsonOptions);

        // assert
        Assert.Contains("\"status\":\"Criado\"", json);
        Assert.Contains("\"isDelivery\":true", json);
        Assert.Contains("\"totalAmount\":42.50", json);
        Assert.Contains("\"notes\":\"Sem cebola\"", json);
        Assert.Contains("\"createdAt\":\"2026-10-13T15:00:00-03:00\"", json);
        Assert.Equal(TimeSpan.FromHours(-3), dto.CreatedAt.Offset);
    }

    [Fact]
    public void UpdateStatusDto_ShouldDeserializeNumericStatus()
    {
        // arrange
        const string json = """
            { "orderId": 10, "status": 1 }
            """;

        // act
        var result = JsonSerializer.Deserialize<UpdateStatusDto>(json, WebJsonOptions);

        // assert
        Assert.NotNull(result);
        Assert.Equal(10, result.OrderId);
        Assert.Equal(OrderStatusEnum.EmPreparacao, result.Status);
    }

    [Fact]
    public void OrderRequestDto_ShouldDeserializeCanonicalFields()
    {
        // arrange
        const string json = """
            { "customerId": 10, "isDelivery": true, "notes": "Sem cebola", "orderItems": [] }
            """;

        // act
        var result = JsonSerializer.Deserialize<OrderRequestDto>(json, WebJsonOptions);

        // assert
        Assert.NotNull(result);
        Assert.True(result.IsDelivery);
        Assert.Equal("Sem cebola", result.Notes);
    }

    [Fact]
    public void OrderStatusChangedMessageDto_ShouldSerializeCanonicalFields()
    {
        // arrange
        var dto = new Prensadao.Application.DTOs.OrderStatusChangedMessageDto
        {
            OrderId = 10,
            Status = OrderStatusEnum.Pronto,
            IsDelivery = true
        };

        // act
        var json = JsonSerializer.Serialize(dto);

        // assert
        Assert.Contains("\"OrderId\":10", json);
        Assert.Contains("\"Status\":2", json);
        Assert.Contains("\"IsDelivery\":true", json);
        Assert.DoesNotContain("CustomerName", json);
        Assert.DoesNotContain("Phone", json);
    }

    [Fact]
    public void OrderStatusChangedMessageDto_ShouldRejectMessage_WhenStatusIsMissing()
    {
        // arrange
        const string json = """
            { "OrderId": 10, "IsDelivery": true }
            """;

        // act
        var act = () => JsonSerializer.Deserialize<Prensadao.Application.DTOs.OrderStatusChangedMessageDto>(json);

        // assert
        Assert.Throws<JsonException>(act);
    }

    [Fact]
    public void ToDto_ShouldUseHistoricalUnitPrice_WhenCurrentProductPriceHasChanged()
    {
        // arrange
        var customer = new Customer("Cliente", "48999999999", "Rua", "Bairro", "123", "Cidade", "Referência", 88000000);
        var order = new Order(true, 50m, string.Empty, 1, DateTime.UtcNow);
        var product = new Product("X Salada", 25m, "Completo");
        var orderItem = new OrderItem(2, 25m, 1, 10);

        product.UpdateDetails(product.Name, product.Enabled, 30m, product.Description);
        typeof(Order).GetProperty(nameof(Order.Customer))!.SetValue(order, customer);
        typeof(OrderItem).GetProperty(nameof(OrderItem.Product))!.SetValue(orderItem, product);
        order.OrderItems.Add(orderItem);

        // act
        var result = OrderResponseDto.ToDto(order);
        var json = JsonSerializer.Serialize(result.OrderItems.Single(), WebJsonOptions);

        // assert
        Assert.Equal(25m, result.OrderItems.Single().UnitPrice);
        Assert.Contains("\"unitPrice\":25", json);
    }
}
