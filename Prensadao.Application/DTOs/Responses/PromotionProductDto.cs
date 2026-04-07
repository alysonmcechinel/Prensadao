namespace Prensadao.Application.DTOs.Responses;

public class PromotionProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public string PromotionName { get; set; } = string.Empty;
    public decimal PromotionalPrice { get; set; }
    public int DiscountPercent { get; set; }
    public string BestDayOfWeek { get; set; } = string.Empty;
}