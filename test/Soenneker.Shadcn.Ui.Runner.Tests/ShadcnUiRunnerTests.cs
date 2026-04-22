using Soenneker.Tests.HostedUnit;

namespace Soenneker.Shadcn.Ui.Runner.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class ShadcnUiRunnerTests : HostedUnitTest
{
    public ShadcnUiRunnerTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
