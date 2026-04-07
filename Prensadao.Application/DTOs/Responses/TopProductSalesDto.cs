using Prensadao.Domain.ReadModels;

namespace Prensadao.Application.DTOs.Responses;

public class TopProductSalesDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }

    public static TopProductSalesDto FromModel(TopProductSalesModels model)
        => new()
        {
            ProductId = model.ProductId,
            ProductName = model.ProductName,
            CurrentPrice = model.CurrentPrice,
            UnitsSold = model.UnitsSold,
            Revenue = model.Revenue
        };

    public static IReadOnlyList<TopProductSalesDto> FromModels(IEnumerable<TopProductSalesModels> models)
        => models.Select(FromModel).ToList();
}