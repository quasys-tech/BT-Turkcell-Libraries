using Xunit;
using Turkcell.BT.Dotnet.Lib;
using Microsoft.Extensions.Configuration;

namespace Turkcell.BT.Dotnet.Tests;

public class BeyondTrustConfigurationProviderTests
{
    [Fact]
    public void Source_Build_ShouldReturnProvider()
    {
        // Arrange
        var options = new BeyondTrustOptions { ApiKey = "test" };
        var source = new BeyondTrustConfigurationSource(options);

        // Act
        var provider = source.Build(new ConfigurationBuilder());

        // Assert
        Assert.NotNull(provider);
        Assert.IsType<BeyondTrustConfigurationProvider>(provider);
    }

    [Fact]
    public void Load_WhenDisabled_DoesNothing_AndDoesNotStartTimer()
    {
        // Arrange
        var options = new BeyondTrustOptions
        {
            Enabled = false,
            ApiKey = "key=abc",
            RefreshIntervalSeconds = 1
        };
        var provider = new BeyondTrustConfigurationProvider(options);

        // Act
        provider.Load();

        // Assert
        var timer = TestReflection.GetPrivateField<Timer>(provider, "_refreshTimer");
        Assert.Null(timer);
    }

    [Fact]
    public void Load_WhenApiKeyMissing_ShouldStillStartTimer()
    {
        // GÜNCELLEME: Yeni mimaride API Key zorunluluğu Load aşamasında kaldırıldı.
        // Çünkü AppUser (OAuth) kullanılıyor olabilir. Bu yüzden Timer başlamalıdır.
        
        // Arrange
        var options = new BeyondTrustOptions
        {
            Enabled = true,
            ApiKey = "", // Key yok
            RefreshIntervalSeconds = 1
        };
        var provider = new BeyondTrustConfigurationProvider(options);

        // Act
        // Service içinde hata alsa bile try-catch ile yutulur, önemli olan timer'ın kurulması.
        try { provider.Load(); } catch { }

        // Assert
        var timer = TestReflection.GetPrivateField<Timer>(provider, "_refreshTimer");
        Assert.NotNull(timer); // Timer ARTIK NULL OLMAMALI
    }

    [Fact]
    public void Load_WhenEnabledAndHasKey_ShouldInitializeTimer()
    {
        // Arrange
        var options = new BeyondTrustOptions
        {
            Enabled = true,
            ApiKey = "key=abc",
            RefreshIntervalSeconds = 60
        };
        var provider = new BeyondTrustConfigurationProvider(options);

        // Act
        try { provider.Load(); } catch { }

        // Assert
        var timer = TestReflection.GetPrivateField<Timer>(provider, "_refreshTimer");
        Assert.NotNull(timer);
    }

    [Fact]
    public void Dispose_ShouldBeSafeAndCleanupTimer()
    {
        // Arrange
        var options = new BeyondTrustOptions { Enabled = true, ApiKey = "key=abc", RefreshIntervalSeconds = 10 };
        var provider = new BeyondTrustConfigurationProvider(options);
        try { provider.Load(); } catch { }

        // Act
        var exception = Record.Exception(() => provider.Dispose());

        // Assert
        Assert.Null(exception);
    }
}