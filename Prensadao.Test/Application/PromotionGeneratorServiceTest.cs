using FakeItEasy;
using NodaTime;
using Prensadao.Application.DTOs.Requests;
using Prensadao.Application.DTOs.Responses;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Services;
using Prensadao.Domain.Interfaces;
using Prensadao.Domain.ReadModels;

namespace Prensadao.Test.Application;

public class PromotionGeneratorServiceTest
{
    private static readonly Instant CurrentInstant = Instant.FromUtc(2026, 10, 18, 15, 0);

    [Fact]
    public async Task GeneratePromotionAsync_ShouldUseDocumentedWindowsAndReturnPromotion_WhenDataIsValid()
    {
        // arrange
        var repository = A.Fake<ISalesAnalyticsRepository>();
        var suggestionService = A.Fake<IPromotionSuggestionService>();
        var clock = A.Fake<IClock>();
        var cancellationToken = new CancellationTokenSource().Token;
        var topProductsStartUtc = new DateTime(2025, 10, 23, 3, 0, 0, DateTimeKind.Utc);
        var bestDayStartUtc = new DateTime(2026, 7, 26, 3, 0, 0, DateTimeKind.Utc);
        var currentBusinessDayStartUtc = new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc);
        var request = new GeneratePromotionRequest
        {
            TopN = 5,
            MinDiscountPercent = 5,
            MaxDiscountPercent = 25
        };
        IReadOnlyList<TopProductSalesModels> products =
        [
            new()
            {
                ProductId = 10,
                ProductName = "X Salada",
                CurrentPrice = 25m,
                UnitsSold = 30,
                Revenue = 750m
            }
        ];

        A.CallTo(() => clock.GetCurrentInstant()).Returns(CurrentInstant);
        A.CallTo(() => repository.GetTopProductsSoldAsync(
                topProductsStartUtc,
                currentBusinessDayStartUtc,
                request.TopN,
                cancellationToken))
            .Returns(products);
        A.CallTo(() => repository.GetBestPromotionDayOfWeekAsync(
                bestDayStartUtc,
                currentBusinessDayStartUtc,
                cancellationToken))
            .Returns(DayOfWeek.Wednesday.ToString());
        A.CallTo(() => suggestionService.SuggestPromotionAsync(
                A<IReadOnlyList<TopProductSalesDto>>.That.Matches(items =>
                    items.Count == 1 &&
                    items[0].ProductId == 10),
                DayOfWeek.Wednesday.ToString(),
                request.MinDiscountPercent,
                request.MaxDiscountPercent,
                cancellationToken))
            .Returns(new PromotionAiSuggestionDto
            {
                ProductId = "10",
                PromotionName = "Quarta do X Salada",
                DiscountPercent = 20
            });

        var service = new PromotionGeneratorService(repository, suggestionService, clock);

        // act
        var result = await service.GeneratePromotionAsync(request, cancellationToken);

        // assert
        Assert.Equal(10, result.ProductId);
        Assert.Equal("X Salada", result.ProductName);
        Assert.Equal(25m, result.CurrentPrice);
        Assert.Equal("Quarta do X Salada", result.PromotionName);
        Assert.Equal(20, result.DiscountPercent);
        Assert.Equal(20m, result.PromotionalPrice);
        Assert.Equal(DayOfWeek.Wednesday.ToString(), result.BestDayOfWeek);

        A.CallTo(() => repository.GetTopProductsSoldAsync(
                topProductsStartUtc,
                currentBusinessDayStartUtc,
                request.TopN,
                cancellationToken))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => repository.GetBestPromotionDayOfWeekAsync(
                bestDayStartUtc,
                currentBusinessDayStartUtc,
                cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(0, 5, 25, "TopN deve ser maior que zero.")]
    [InlineData(10, -1, 25, "Limites de desconto inválidos.")]
    [InlineData(10, 5, 0, "Limites de desconto inválidos.")]
    [InlineData(10, 30, 25, "Limites de desconto inválidos.")]
    public async Task GeneratePromotionAsync_ShouldRejectInvalidRequest(
        int topN,
        int minDiscountPercent,
        int maxDiscountPercent,
        string expectedMessage)
    {
        // arrange
        var repository = A.Fake<ISalesAnalyticsRepository>();
        var suggestionService = A.Fake<IPromotionSuggestionService>();
        var clock = A.Fake<IClock>();
        var service = new PromotionGeneratorService(repository, suggestionService, clock);
        var request = new GeneratePromotionRequest
        {
            TopN = topN,
            MinDiscountPercent = minDiscountPercent,
            MaxDiscountPercent = maxDiscountPercent
        };

        // act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GeneratePromotionAsync(request, CancellationToken.None));

        // assert
        Assert.Equal(expectedMessage, exception.Message);
        A.CallTo(() => repository.GetTopProductsSoldAsync(
                A<DateTime>._,
                A<DateTime>._,
                A<int>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task GeneratePromotionAsync_ShouldStopBeforeDayAndAiQueries_WhenNoProductsAreFound()
    {
        // arrange
        var repository = A.Fake<ISalesAnalyticsRepository>();
        var suggestionService = A.Fake<IPromotionSuggestionService>();
        var clock = A.Fake<IClock>();
        A.CallTo(() => clock.GetCurrentInstant()).Returns(CurrentInstant);
        A.CallTo(() => repository.GetTopProductsSoldAsync(
                A<DateTime>._,
                A<DateTime>._,
                A<int>._,
                A<CancellationToken>._))
            .Returns(Array.Empty<TopProductSalesModels>());

        var service = new PromotionGeneratorService(repository, suggestionService, clock);

        // act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GeneratePromotionAsync(new GeneratePromotionRequest(), CancellationToken.None));

        // assert
        Assert.Equal("Não há vendas suficientes para gerar promoção.", exception.Message);
        A.CallTo(() => repository.GetBestPromotionDayOfWeekAsync(
                A<DateTime>._,
                A<DateTime>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => suggestionService.SuggestPromotionAsync(
                A<IReadOnlyList<TopProductSalesDto>>._,
                A<string>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData("invalid", "ProductId sugerido pela IA é inválido.")]
    [InlineData("999", "Produto sugerido não encontrado entre os mais vendidos.")]
    public async Task GeneratePromotionAsync_ShouldRejectInvalidAiProduct(
        string suggestedProductId,
        string expectedMessage)
    {
        // arrange
        var repository = A.Fake<ISalesAnalyticsRepository>();
        var suggestionService = A.Fake<IPromotionSuggestionService>();
        var clock = A.Fake<IClock>();
        IReadOnlyList<TopProductSalesModels> products =
        [
            new() { ProductId = 10, ProductName = "X Salada", CurrentPrice = 25m }
        ];

        A.CallTo(() => clock.GetCurrentInstant()).Returns(CurrentInstant);
        A.CallTo(() => repository.GetTopProductsSoldAsync(
                A<DateTime>._,
                A<DateTime>._,
                A<int>._,
                A<CancellationToken>._))
            .Returns(products);
        A.CallTo(() => repository.GetBestPromotionDayOfWeekAsync(
                A<DateTime>._,
                A<DateTime>._,
                A<CancellationToken>._))
            .Returns(DayOfWeek.Friday.ToString());
        A.CallTo(() => suggestionService.SuggestPromotionAsync(
                A<IReadOnlyList<TopProductSalesDto>>._,
                A<string>._,
                A<int>._,
                A<int>._,
                A<CancellationToken>._))
            .Returns(new PromotionAiSuggestionDto
            {
                ProductId = suggestedProductId,
                PromotionName = "Promoção",
                DiscountPercent = 10
            });

        var service = new PromotionGeneratorService(repository, suggestionService, clock);

        // act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GeneratePromotionAsync(new GeneratePromotionRequest(), CancellationToken.None));

        // assert
        Assert.Equal(expectedMessage, exception.Message);
    }
}
