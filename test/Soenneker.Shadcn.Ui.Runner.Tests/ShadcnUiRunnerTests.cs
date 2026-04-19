using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Shadcn.Ui.Runner.Tests;

[Collection("Collection")]
public sealed class ShadcnUiRunnerTests : FixturedUnitTest
{
    public ShadcnUiRunnerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
