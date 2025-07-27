namespace Prensadao.Application.Helpers;

public static class DateTimeExtensions
{
    public static DateTime ToBrasil(this DateTime dateTime)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);
    }
}
