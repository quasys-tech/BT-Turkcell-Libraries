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
        "BEYONDTRUST_CLIENT_SECRET",
        "BEYONDTRUST_IGNORE_SSL_ERRORS",
        "BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED",
        "BEYONDTRUST_ALL_SECRETS_ENABLED",
        "BEYONDTRUST_REFRESH_INTERVAL",
        "BT_REFRESH_TIME"
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
    public void AddBeyondTrustSecrets_DoesNotAddSource_WhenUseAppUserIsMissing()
    {
        SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        SetEnvironmentVariable("BEYONDTRUST_API_KEY", "raw-api-key-value");

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        builder.AddBeyondTrustSecrets();

        Assert.DoesNotContain(builder.Sources, source => source is BeyondTrustConfigurationSource);
    }

    [Fact]
    public void AddBeyondTrustSecrets_DoesNotAddSource_WhenOAuthFieldsAreMissing()
    {
        SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
        SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "true");
        SetEnvironmentVariable("BEYONDTRUST_CLIENT_ID", null);
        SetEnvironmentVariable("BEYONDTRUST_CLIENT_SECRET", null);

        var builder = new ConfigurationBuilder().AddEnvironmentVariables();

        builder.AddBeyondTrustSecrets();

        Assert.DoesNotContain(builder.Sources, source => source is BeyondTrustConfigurationSource);
    }

    [Fact]
    public void BindOptions_UsesCanonicalRefreshInterval_WhenCanonicalValueExists()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BEYONDTRUST_REFRESH_INTERVAL"] = "240"
        });

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(240, options.RefreshIntervalSeconds);
    }

    [Fact]
    public void BindOptions_UsesLegacyRefreshIntervalAlias_WhenCanonicalValueMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BT_REFRESH_TIME"] = "120"
        });

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(120, options.RefreshIntervalSeconds);
    }

    [Fact]
    public void BindOptions_UsesCanonicalRefreshInterval_WhenBothCanonicalAndLegacyExist()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BEYONDTRUST_REFRESH_INTERVAL"] = "240",
            ["BT_REFRESH_TIME"] = "120"
        });

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(240, options.RefreshIntervalSeconds);
    }

    [Fact]
    public void BindOptions_UsesDefaultRefreshInterval_WhenCanonicalAndLegacyAreMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(BeyondTrustOptions.DefaultRefreshIntervalSeconds, options.RefreshIntervalSeconds);
    }

    [Fact]
    public void BindOptions_UsesDefaultRefreshInterval_WhenLegacyAliasIsInvalidAndCanonicalMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BT_REFRESH_TIME"] = "not-an-integer"
        });

        var options = BeyondTrustExtensions.BindOptions(configuration);

        Assert.Equal(BeyondTrustOptions.DefaultRefreshIntervalSeconds, options.RefreshIntervalSeconds);
    }

    [Fact]
    public void BindOptions_Throws_WhenCanonicalRefreshIntervalIsInvalid_WithoutLegacyAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BEYONDTRUST_REFRESH_INTERVAL"] = "not-an-integer"
        });

        var exception = Assert.Throws<InvalidOperationException>(() => BeyondTrustExtensions.BindOptions(configuration));

        Assert.Contains("BEYONDTRUST_REFRESH_INTERVAL", exception.Message);
    }

    [Fact]
    public void BindOptions_Throws_WhenCanonicalRefreshIntervalIsInvalid()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["BEYONDTRUST_REFRESH_INTERVAL"] = "not-an-integer",
            ["BT_REFRESH_TIME"] = "120"
        });

        var exception = Assert.Throws<InvalidOperationException>(() => BeyondTrustExtensions.BindOptions(configuration));

        Assert.Contains("BEYONDTRUST_REFRESH_INTERVAL", exception.Message);
    }

    [Theory]
    [InlineData("BEYONDTRUST_USE_APP_USER", "not-a-boolean")]
    [InlineData("BEYONDTRUST_IGNORE_SSL_ERRORS", "not-a-boolean")]
    [InlineData("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "not-a-boolean")]
    [InlineData("BEYONDTRUST_ALL_SECRETS_ENABLED", "not-a-boolean")]
    [InlineData("BEYONDTRUST_ENABLED", "not-a-boolean")]
    public void BindOptions_Throws_WhenSharedBooleanValueIsInvalid(string key, string value)
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            [key] = value
        });

        var exception = Assert.Throws<InvalidOperationException>(() => BeyondTrustExtensions.BindOptions(configuration));

        Assert.Contains(key, exception.Message);
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

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
