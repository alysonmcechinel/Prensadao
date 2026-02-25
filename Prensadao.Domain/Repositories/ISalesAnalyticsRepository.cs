using Prensadao.Domain.ReadModels;

namespace Prensadao.Domain.Interfaces;

public interface ISalesAnalyticsRepository
{
    Task<IReadOnlyList<TopProductSalesModels>> GetTopProductsSoldAsync(DateTime fromUtc, DateTime toUtc, int topN, CancellationToken cancellationToken);
    Task<string> GetBestPromotionDayOfWeekAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken);
}