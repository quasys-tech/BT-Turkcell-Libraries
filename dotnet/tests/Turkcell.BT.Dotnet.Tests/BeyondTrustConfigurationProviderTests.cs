using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

public sealed class BeyondTrustConfigurationProviderTests
{
    [Fact]
    public void Source_Build_ReturnsProvider()
    {
        var source = new BeyondTrustConfigurationSource(new BeyondTrustOptions());

        var provider = source.Build(new ConfigurationBuilder());

        Assert.IsType<BeyondTrustConfigurationProvider>(provider);
    }

    [Fact]
    public void Load_WhenDisabled_DoesNotStartTimer()
    {
        var provider = new BeyondTrustConfigurationProvider(
            new BeyondTrustOptions { Enabled = false, RefreshIntervalSeconds = 1 },
            () => Task.FromResult(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)));

        provider.Load();

        Assert.Null(TestInfrastructure.GetPrivateField<Timer>(provider, "_refreshTimer"));
    }

    [Fact]
    public void Refresh_ReplacesSnapshotAtomically_AndRemovesStaleKeys()
    {
        var calls = 0;
        var provider = new BeyondTrustConfigurationProvider(
            new BeyondTrustOptions { Enabled = true, RefreshIntervalSeconds = 0 },
            () =>
            {
                calls++;
                return Task.FromResult(calls == 1
                    ? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["bt.acc.Sys.Account"] = "first-value",
                        ["bt.safe.Team.Api.password"] = "first-password"
                    }
                    : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["bt.acc.Sys.Account"] = "second-value"
                    });
            });

        provider.Load();
        TestInfrastructure.InvokePrivateMethod(provider, "DoRefresh", new object?[] { null });

        Assert.True(provider.TryGet("bt.acc.Sys.Account", out var accountValue));
        Assert.Equal("second-value", accountValue);
        Assert.False(provider.TryGet("bt.safe.Team.Api.password", out _));
    }

    [Fact]
    public void Refresh_Failure_KeepsLastSuccessfulSnapshot()
    {
        var calls = 0;
        var provider = new BeyondTrustConfigurationProvider(
            new BeyondTrustOptions { Enabled = true, RefreshIntervalSeconds = 0 },
            () =>
            {
                calls++;
                if (calls == 1)
                {
                    return Task.FromResult(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["bt.acc.Sys.Account"] = "stable-value"
                    });
                }

                throw new InvalidOperationException("simulated failure");
            });

        provider.Load();
        TestInfrastructure.InvokePrivateMethod(provider, "DoRefresh", new object?[] { null });

        Assert.True(provider.TryGet("bt.acc.Sys.Account", out var accountValue));
        Assert.Equal("stable-value", accountValue);
        Assert.DoesNotContain("ERROR_", accountValue ?? string.Empty);
    }

    [Fact]
    public void Load_WithRefreshInterval_StartsTimer()
    {
        var provider = new BeyondTrustConfigurationProvider(
            new BeyondTrustOptions { Enabled = true, RefreshIntervalSeconds = 5 },
            () => Task.FromResult(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)));

        provider.Load();

        Assert.NotNull(TestInfrastructure.GetPrivateField<Timer>(provider, "_refreshTimer"));
        provider.Dispose();
    }
}
