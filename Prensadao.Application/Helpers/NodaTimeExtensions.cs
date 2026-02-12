using NodaTime;

namespace Prensadao.Application.Helpers;

public static class NodaTimeExtensions
{
    private const string DefaultTimeZone = "America/Sao_Paulo";

    public static DateTime NowUtc()
    {
        Instant instant = SystemClock.Instance.GetCurrentInstant();
        return instant.ToDateTimeUtc();
    }

    public static DateTime LocalTime(this DateTime dataUtc)
    {
        var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(dataUtc, DateTimeKind.Utc));
        var zone = DateTimeZoneProviders.Tzdb[DefaultTimeZone];
        var zoned = instant.InZone(zone);
        return zoned.ToDateTimeUnspecified(); // evita alterar para UTC
    }
}
