using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

public sealed class BeyondTrustConfigurationTests : IDisposable
{
    private static readonly string[] EnvironmentKeys =
    [
        "BEYONDTRUST_ENABLED",
        "BEYONDTRUST_API_URL",
        "BEYONDTRUST_USE_APP_USER",
        "BEYONDTRUST_API_KEY",
        "BEYONDTRUST_RUNAS_USER",
        "BEYONDTRUST_CLIENT_ID",
        "BEYONDTRUST_CLIENT_SECRET"
    ];

    [Fact]
    public void AddBeyondTrustSecrets_AddsSource_ForClassicApiMode()
    {
        SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "false");
        SetEnvironmentVariable("BEYONDTRUST_API_KEY", "raw-api-key-value");
        SetEnvironmentVariable("BEYONDTRUST_RUNAS_USER", "svc-demo");

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        builder.AddBeyondTrustSecrets();

        Assert.Contains(builder.Sources, source => source is BeyondTrustConfigurationSource);
    }

    [Fact]
    public void AddBeyondTrustSecrets_AddsSource_ForOAuthMode()
    {
        SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "true");
        SetEnvironmentVariable("BEYONDTRUST_CLIENT_ID", "client-id");
        SetEnvironmentVariable("BEYONDTRUST_CLIENT_SECRET", "client-secret");

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        builder.AddBeyondTrustSecrets();

        Assert.Contains(builder.Sources, source => source is BeyondTrustConfigurationSource);
    }

    [Fact]
    public void AddBeyondTrustSecrets_DoesNotAddSource_WhenValidationFails()
    {
        SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "false");
        SetEnvironmentVariable("BEYONDTRUST_API_KEY", null);

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        builder.AddBeyondTrustSecrets();

        Assert.DoesNotContain(builder.Sources, source => source is BeyondTrustConfigurationSource);
    }

    [Fact]
    public void BindOptions_UsesLegacyRefreshIntervalAlias_WhenCanonicalValueMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BEYONDTRUST_ENABLED"] = "true",
                ["BEYONDTRUST_API_URL"] = "https://pam.example.com/BeyondTrust/api/public/v3",
                ["BEYONDTRUST_USE_APP_USER"] = "false",
                ["BEYONDTRUST_API_KEY"] = "raw-api-key-value",
                ["BT_REFRESH_TIME"] = "120"
            })
            .Build();

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(120, options.RefreshIntervalSeconds);
    }

    public void Dispose()
    {
        foreach (var environmentKey in EnvironmentKeys)
        {
            SetEnvironmentVariable(environmentKey, null);
        }
    }

    private static void SetEnvironmentVariable(string key, string? value)
    {
        Environment.SetEnvironmentVariable(key, value);
    }
}
