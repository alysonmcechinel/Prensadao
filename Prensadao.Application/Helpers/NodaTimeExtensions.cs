using NodaTime;

namespace Prensadao.Application.Helpers;

public static class NodaTimeExtensions
{
    private const string DefaultTimeZone = "America/Sao_Paulo";

    public static DateTime NowUtc(this IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        Instant instant = clock.GetCurrentInstant();
        return instant.ToDateTimeUtc();
    }

    public static DateTimeOffset ToSaoPauloDateTimeOffset(this DateTime dateTimeUtc)
    {
        var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc));
        var zone = DateTimeZoneProviders.Tzdb[DefaultTimeZone];
        return instant.InZone(zone).ToDateTimeOffset();
    }
}
