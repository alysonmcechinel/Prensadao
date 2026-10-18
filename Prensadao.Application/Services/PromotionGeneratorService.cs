using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Interfaces;
using NodaTime;

namespace Prensadao.Application.Services;

public class PromotionGeneratorService : IPromotionGenerator
{
    private const int TopProductsLookbackDays = 360;
    private const int BestDayLookbackDays = 84;

    private readonly ISalesAnalyticsRepository _salesAnalyticsRepository;
    private readonly IPromotionSuggestionService _promotionSuggestionService;
    private readonly IClock _clock;

    public PromotionGeneratorService(
        ISalesAnalyticsRepository salesAnalyticsRepository,
        IPromotionSuggestionService promotionSuggestionService,
        IClock clock)
    {
        _salesAnalyticsRepository = salesAnalyticsRepository;
        _promotionSuggestionService = promotionSuggestionService;
        _clock = clock;
    }

    public async Task<PromotionProductDto> GeneratePromotionAsync(GeneratePromotionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.TopN <= 0)
            throw new ArgumentException("TopN deve ser maior que zero.");

        if (request.MinDiscountPercent < 0 || request.MaxDiscountPercent <= 0 || request.MinDiscountPercent > request.MaxDiscountPercent)
            throw new ArgumentException("Limites de desconto inválidos.");

        var currentInstant = _clock.GetCurrentInstant();
        var topProductsWindow = currentInstant.GetCompletedBusinessDaysWindowUtc(TopProductsLookbackDays);
        var bestDayWindow = currentInstant.GetCompletedBusinessDaysWindowUtc(BestDayLookbackDays);

        var topProducts = await _salesAnalyticsRepository.GetTopProductsSoldAsync(
            topProductsWindow.FromInclusiveUtc,
            topProductsWindow.ToExclusiveUtc,
            request.TopN,
            cancellationToken);
        if (topProducts.Count == 0)
            throw new InvalidOperationException("Não há vendas suficientes para gerar promoção.");

        var topProductsDTO = TopProductSalesDto.FromModels(topProducts);

        var bestDayOfWeek = await _salesAnalyticsRepository.GetBestPromotionDayOfWeekAsync(
            bestDayWindow.FromInclusiveUtc,
            bestDayWindow.ToExclusiveUtc,
            cancellationToken);

        var aiSuggestion = await _promotionSuggestionService.SuggestPromotionAsync(
            topProductsDTO,
            bestDayOfWeek,
            request.MinDiscountPercent,
            request.MaxDiscountPercent,
            cancellationToken);

        if (!int.TryParse(aiSuggestion.ProductId, out var suggestedProductId))
            throw new InvalidOperationException("ProductId sugerido pela IA é inválido.");

        var selectedProduct = topProducts.FirstOrDefault(x => x.ProductId == suggestedProductId)
            ?? throw new InvalidOperationException("Produto sugerido não encontrado entre os mais vendidos.");

        var safeDiscount = Math.Clamp(aiSuggestion.DiscountPercent, request.MinDiscountPercent, request.MaxDiscountPercent);
        var promotionalPrice = Math.Round(selectedProduct.CurrentPrice * (1 - safeDiscount / 100m), 2, MidpointRounding.AwayFromZero);

        if (promotionalPrice < 0)
            throw new InvalidOperationException("Preço promocional inválido.");

        return new PromotionProductDto
        {
            ProductId = selectedProduct.ProductId,
            ProductName = selectedProduct.ProductName,
            CurrentPrice = selectedProduct.CurrentPrice,
            PromotionName = aiSuggestion.PromotionName,
            DiscountPercent = safeDiscount,
            PromotionalPrice = promotionalPrice,
            BestDayOfWeek = bestDayOfWeek
        };
    }
}
