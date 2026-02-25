using Prensadao.Application.DTOs.Responses;

namespace Prensadao.Application.Interfaces;

public interface IPromotionSuggestionService
{
    Task<PromotionAiSuggestionDto> SuggestPromotionAsync(
        IReadOnlyList<TopProductSalesDto> topProducts,
        string bestDayOfWeek,
        int minDiscountPercent,
        int maxDiscountPercent,
        CancellationToken cancellationToken);
}