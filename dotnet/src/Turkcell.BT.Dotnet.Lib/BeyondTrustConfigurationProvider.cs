using Microsoft.Extensions.Configuration;

namespace Turkcell.BT.Dotnet.Lib;

/// <summary>
/// BeyondTrust verilerini ConfigurationSource üzerinden Build eder.
/// </summary>
public class BeyondTrustConfigurationSource : IConfigurationSource
{
    private readonly BeyondTrustOptions _options;
    public BeyondTrustConfigurationSource(BeyondTrustOptions options) => _options = options;

    public IConfigurationProvider Build(IConfigurationBuilder builder) 
        => new BeyondTrustConfigurationProvider(_options);
}

/// <summary>
/// BeyondTrust API'sinden verileri çeken ve periyodik olarak güncelleyen ConfigurationProvider.
/// </summary>
public class BeyondTrustConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly BeyondTrustOptions _options;
    private Timer? _refreshTimer;
    private readonly object _lock = new();

    public BeyondTrustConfigurationProvider(BeyondTrustOptions options) => _options = options;

    /// <summary>
    /// Uygulama ilk ayağa kalktığında veriyi yükler ve yenileme zamanlayıcısını başlatır.
    /// </summary>
    public override void Load()
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return;
        }

        LoadDataInternal();

        if (_options.RefreshIntervalSeconds > 0)
        {
            var interval = TimeSpan.FromSeconds(_options.RefreshIntervalSeconds);
            
            // Timer sızıntısını önlemek için varsa temizle
            _refreshTimer?.Dispose();
            
            // Periyodik yenileme başlatılır
            _refreshTimer = new Timer(DoRefresh, null, interval, interval);
            
            // Turkcell operasyonel log standartları için bilgi
            Console.WriteLine($"✅ [Turkcell.BT.BeyondTrust] Background refresh started. Interval: {_options.RefreshIntervalSeconds}s");
        }
    }

    private void DoRefresh(object? state)
    {
        try
        {
            // Thread-safety: Aynı anda birden fazla yenileme tetiklenmesini engelle
            lock (_lock)
            {
                using var service = new BeyondTrustService(_options);
                var newData = service.FetchAllSecretsAsync().GetAwaiter().GetResult();

                if (newData != null && newData.Count > 0)
                {
                    Data = newData;
                    
                    // Uygulama genelinde konfigürasyonun değiştiğini bildirir
                    OnReload();
                }
            }
        }
        catch (Exception ex)
        {
            // Yenileme başarısız olsa bile uygulama eski verilerle çalışmaya devam eder
            Console.WriteLine($"⚠️ [Turkcell.BT.BeyondTrust] Refresh failed at {DateTime.Now:HH:mm:ss}. Keeping stale data. Error: {ex.Message}");
        }
    }

    private void LoadDataInternal()
    {
        try
        {
            using var service = new BeyondTrustService(_options);
            var data = service.FetchAllSecretsAsync().GetAwaiter().GetResult();
            
            if (data != null && data.Count > 0)
            {
                Data = data;
            }
        }
        catch (Exception ex)
        {
            // İlk yükleme hatası kritik olabilir, ancak uygulamanın çökmesini engellemek için sadece loglanır
            Console.WriteLine($"❌ [Turkcell.BT.BeyondTrust] Initial load error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}