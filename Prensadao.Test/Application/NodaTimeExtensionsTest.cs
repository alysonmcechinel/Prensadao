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

    [Fact]
    public void GetCompletedBusinessDaysWindowUtc_ShouldExcludeCurrentBusinessDate()
    {
        // arrange
        var currentInstant = Instant.FromUtc(2026, 10, 18, 15, 0);

        // act
        var result = currentInstant.GetCompletedBusinessDaysWindowUtc(84);

        // assert
        Assert.Equal(new DateTime(2026, 7, 26, 3, 0, 0, DateTimeKind.Utc), result.FromInclusiveUtc);
        Assert.Equal(new DateTime(2026, 10, 18, 3, 0, 0, DateTimeKind.Utc), result.ToExclusiveUtc);
    }

    [Fact]
    public void GetCompletedBusinessDaysWindowUtc_ShouldUsePreviousLocalDate_WhenUtcDateAlreadyChanged()
    {
        // arrange
        var currentInstant = Instant.FromUtc(2026, 10, 18, 2, 30);

        // act
        var result = currentInstant.GetCompletedBusinessDaysWindowUtc(1);

        // assert
        Assert.Equal(new DateTime(2026, 10, 16, 3, 0, 0, DateTimeKind.Utc), result.FromInclusiveUtc);
        Assert.Equal(new DateTime(2026, 10, 17, 3, 0, 0, DateTimeKind.Utc), result.ToExclusiveUtc);
    }

    [Fact]
    public void GetCompletedBusinessDaysWindowUtc_ShouldRejectNonPositiveDays()
    {
        // arrange
        var currentInstant = Instant.FromUtc(2026, 10, 18, 15, 0);

        // act
        Action act = () => currentInstant.GetCompletedBusinessDaysWindowUtc(0);

        // assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("days", exception.ParamName);
    }
}
