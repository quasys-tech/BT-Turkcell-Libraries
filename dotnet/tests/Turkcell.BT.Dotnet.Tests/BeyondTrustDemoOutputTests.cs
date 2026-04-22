using Turkcell.BT.Dotnet.Demo;

namespace Turkcell.BT.Dotnet.Tests;

public sealed class BeyondTrustDemoOutputTests
{
    [Fact]
    public void BuildExampleOutputLines_LogsConfiguredSafeUsernameSample()
    {
        var snapshot = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["bt.safe.SampleFolder.SampleTitle.username"] = "demo-user"
        };

        var lines = DemoExampleOutput.BuildExampleOutputLines(
            snapshot,
            exampleAccountKey: null,
            exampleSafePasswordKey: null,
            exampleSafeUsernameKey: "bt.safe.SampleFolder.SampleTitle.username");

        Assert.Contains("Secret Safe Username Sample (bt.safe.SampleFolder.SampleTitle.username) = demo-user", lines);
    }

    [Fact]
    public void BuildExampleOutputLines_ShowsSkipMessage_WhenSafeUsernameExampleIsNotSet()
    {
        var lines = DemoExampleOutput.BuildExampleOutputLines(
            new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase),
            exampleAccountKey: null,
            exampleSafePasswordKey: null,
            exampleSafeUsernameKey: null);

        Assert.Contains("BT_EXAMPLE_SAFE_USERNAME not set; skipping example username output", lines);
    }
}
