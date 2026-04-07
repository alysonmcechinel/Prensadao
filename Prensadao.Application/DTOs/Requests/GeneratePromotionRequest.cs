namespace Prensadao.Application.DTOs.Requests;

public class GeneratePromotionRequest
{
    public int TopN { get; set; } = 10;
    public int MaxDiscountPercent { get; set; } = 25;
    public int MinDiscountPercent { get; set; } = 5;
}
