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
        // Temiz bir ortam için önce sil
        Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", null);
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", null);
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", null);
        Environment.SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", null);

        // Yeni değerleri set et
        Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.test");
        Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", "PS-Auth key=test;");
        Environment.SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "false"); // API Key modunu zorla

        try
        {
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            builder.AddBeyondTrustSecrets();

            // Source eklenmiş mi kontrol et
            Assert.Contains(builder.Sources, s => s is BeyondTrustConfigurationSource);
        }
        finally
        {
            // Temizlik
            Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", null);
            Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", null);
            Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", null);
            Environment.SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", null);
        }
    }
}