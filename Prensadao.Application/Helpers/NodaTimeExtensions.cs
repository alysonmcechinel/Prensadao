using NodaTime;

namespace Prensadao.Application.Helpers;

public static class NodaTimeExtensions
{
    public const string BusinessTimeZoneId = "America/Sao_Paulo";

    public static DateTime NowUtc(this IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        Instant instant = clock.GetCurrentInstant();
        return instant.ToDateTimeUtc();
    }

    public static DateTimeOffset ToSaoPauloDateTimeOffset(this DateTime dateTimeUtc)
    {
        var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc));
        var zone = DateTimeZoneProviders.Tzdb[BusinessTimeZoneId];
        return instant.InZone(zone).ToDateTimeOffset();
    }

    public static (DateTime FromInclusiveUtc, DateTime ToExclusiveUtc) GetCompletedBusinessDaysWindowUtc(
        this Instant currentInstant,
        int days)
    {
        if (days <= 0)
            throw new ArgumentOutOfRangeException(nameof(days), "A quantidade de dias deve ser maior que zero.");

        var zone = DateTimeZoneProviders.Tzdb[BusinessTimeZoneId];
        var currentBusinessDate = currentInstant.InZone(zone).Date;
        var toExclusive = currentBusinessDate.AtStartOfDayInZone(zone).ToInstant();
        var fromInclusive = currentBusinessDate.PlusDays(-days).AtStartOfDayInZone(zone).ToInstant();

        return (fromInclusive.ToDateTimeUtc(), toExclusive.ToDateTimeUtc());
    }
}
