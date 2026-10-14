using System.Text.Json;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;

namespace Prensadao.Test.Application;

public class OrderDtoContractTest
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void OrderResponseDto_ShouldSerializeTotalAmountWithoutLegacyValue()
    {
        // arrange
        var dto = new OrderResponseDto(
            1,
            OrderStatusEnum.Criado,
            true,
            42.50m,
            "Sem cebola",
            10,
            "Cliente",
            DateTime.UtcNow,
            []);

        // act
        var json = JsonSerializer.Serialize(dto, WebJsonOptions);

        // assert
        Assert.Contains("\"totalAmount\":42.50", json);
        Assert.DoesNotContain("\"value\"", json);
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
        Assert.DoesNotContain("\"productValue\"", json);
    }
}
