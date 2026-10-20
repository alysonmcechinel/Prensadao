using System.Text.Json;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Test.Application;

public class ProductDtoContractTest
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void ProductRequestDto_ShouldDeserializePrice_WhenUsingCanonicalContract()
    {
        // arrange
        const string json = """
            { "name": "X Salada", "enabled": true, "price": 25.90, "description": "Completo" }
            """;

        // act
        var result = JsonSerializer.Deserialize<ProductRequestDto>(json, WebJsonOptions);

        // assert
        Assert.NotNull(result);
        Assert.Equal(25.90m, result.Price);
    }

    [Fact]
    public void ProductResponseDto_ShouldSerializePrice()
    {
        // arrange
        var dto = new ProductResponseDto(1, "X Salada", true, 25.90m, "Completo");

        // act
        var json = JsonSerializer.Serialize(dto, WebJsonOptions);

        // assert
        Assert.Contains("\"price\":25.90", json);
    }
}
