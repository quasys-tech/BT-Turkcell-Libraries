using Xunit;
using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

public class BeyondTrustConfigurationTests
{
    [Fact]
    public void Provider_Load_WhenDisabled_ShouldEmptyData()
    {
        var options = new BeyondTrustOptions { Enabled = false };
        var provider = new BeyondTrustConfigurationProvider(options);
        provider.Load();
        Assert.Empty(provider.GetChildKeys([], null));
    }

    [Fact]
    public void Extensions_AddBeyondTrustSecrets_ShouldAddSource()
    {
        Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.test");
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", "PS-Auth key=test;");

        try
        {
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            builder.AddBeyondTrustSecrets();

            Assert.Contains(builder.Sources, s => s is BeyondTrustConfigurationSource);
        }
        finally
        {
            Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", null);
            Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", null);
            Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", null);
        }
    }
}
