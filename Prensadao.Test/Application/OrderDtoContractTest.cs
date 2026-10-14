using System.Text.Json;
using Prensadao.Application.DTOs.Responses;
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
}
