using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Prensadao.Infra.Persistence.Converters;

public class InstantConverter : ValueConverter<Instant, DateTime>
{
    public InstantConverter() : base(
        Instant => Instant.ToDateTimeUtc(),
        DateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(DateTime, DateTimeKind.Utc)))
        { }
}
