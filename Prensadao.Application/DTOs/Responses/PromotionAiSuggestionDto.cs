namespace Prensadao.Application.DTOs.Responses;

public class PromotionAiSuggestionDto
{
    public string ProductId { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
}