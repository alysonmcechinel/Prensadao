using Microsoft.EntityFrameworkCore;
using Prensadao.Domain.Interfaces;
using Prensadao.Domain.ReadModels;

namespace Prensadao.Infra.Persistence.Repositories;

public class SalesAnalyticsRepository : ISalesAnalyticsRepository
{
    private readonly PrensadaoDbContext _context;

    public SalesAnalyticsRepository(PrensadaoDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TopProductSalesModels>> GetTopProductsSoldAsync(DateTime fromUtc, DateTime toUtc, int topN, CancellationToken cancellationToken)
    {
        var sales = await _context.OrderItems
            .Where(oi => oi.Order.DateOrder >= fromUtc && oi.Order.DateOrder <= toUtc)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.Value })
            .Select(group => new TopProductSalesModels
            {
                ProductId = group.Key.ProductId,
                ProductName = group.Key.Name,
                CurrentPrice = group.Key.Value,
                UnitsSold = group.Sum(x => x.Quantity),
                Revenue = group.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.UnitsSold)
            .ThenByDescending(x => x.Revenue)
            .Take(topN)
            .ToListAsync(cancellationToken);

        return sales;
    }

    public async Task<string> GetBestPromotionDayOfWeekAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken)
    {
        var grouped = await _context.OrderItems
            .Where(oi => oi.Order.DateOrder >= fromUtc && oi.Order.DateOrder <= toUtc)
            .GroupBy(oi => oi.Order.DateOrder.DayOfWeek)
            .Select(group => new
            {
                Day = group.Key,
                TotalRevenue = group.Sum(x => x.UnitPrice * x.Quantity),
                TotalUnits = group.Sum(x => x.Quantity),
                Occurrences = group.Select(x => x.Order.DateOrder.Date).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        if (grouped.Count == 0)
            return DayOfWeek.Friday.ToString();

        var bestDay = grouped
            .Select(x => new
            {
                x.Day,
                AvgRevenue = x.Occurrences == 0 ? 0 : x.TotalRevenue / x.Occurrences,
                x.TotalUnits
            })
            .OrderByDescending(x => x.AvgRevenue)
            .ThenByDescending(x => x.TotalUnits)
            .First();

        return bestDay.Day.ToString();
    }
}