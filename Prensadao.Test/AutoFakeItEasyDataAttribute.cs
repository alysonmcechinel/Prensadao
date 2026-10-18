using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using FakeItEasy;
using NodaTime;

namespace Prensadao.Test;

public class AutoFakeItEasyDataAttribute : AutoDataAttribute
{
    public AutoFakeItEasyDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture()
                .Customize(new AutoFakeItEasyCustomization
                {
                    ConfigureMembers = true // Faz com que membros das interfaces sejam simulados automaticamente
                });

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var clock = A.Fake<IClock>();
            A.CallTo(() => clock.GetCurrentInstant()).Returns(Instant.FromUtc(2026, 1, 1, 12, 0));
            fixture.Inject(clock);

            return fixture;
        })
        { }
}
