namespace Prensadao.Seeder;

public sealed record LocalDatabaseSeedOptions
{
    public const int DefaultCustomerCount = 2_000;
    public const int DefaultProductCount = 100;
    public const int DefaultOrderCount = 50_000;
    public const int DefaultHistoryDays = 365;
    public const int DefaultRandomSeed = 20_261_108;
    public const int DefaultBatchSize = 1_000;

    public int CustomerCount { get; init; } = DefaultCustomerCount;
    public int ProductCount { get; init; } = DefaultProductCount;
    public int OrderCount { get; init; } = DefaultOrderCount;
    public int HistoryDays { get; init; } = DefaultHistoryDays;
    public int RandomSeed { get; init; } = DefaultRandomSeed;
    public int BatchSize { get; init; } = DefaultBatchSize;

    public void Validate()
    {
        ValidateRange(CustomerCount, 1, 100_000, nameof(CustomerCount));
        ValidateRange(ProductCount, 1, 10_000, nameof(ProductCount));
        ValidateRange(OrderCount, 1, 5_000_000, nameof(OrderCount));
        ValidateRange(HistoryDays, 1, 3_650, nameof(HistoryDays));
        ValidateRange(BatchSize, 100, 10_000, nameof(BatchSize));
    }

    private static void ValidateRange(int value, int minimum, int maximum, string propertyName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentOutOfRangeException(
                propertyName,
                value,
                $"O valor deve estar entre {minimum:N0} e {maximum:N0}.");
        }
    }
}
