using FakeItEasy;
using NodaTime;
using Prensadao.Application.Helpers;

namespace Prensadao.Test.Application;

public class NodaTimeExtensionsTest
{
    [Fact]
    public void NowUtc_ShouldReturnInjectedClockInstantAsUtcDateTime()
    {
        // arrange
        var clock = A.Fake<IClock>();
        A.CallTo(() => clock.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 10, 18, 15, 30));

        // act
        var result = clock.NowUtc();

        // assert
        Assert.Equal(new DateTime(2026, 10, 18, 15, 30, 0, DateTimeKind.Utc), result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }
}
