using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;

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

            return fixture;
        })
        { }
}
