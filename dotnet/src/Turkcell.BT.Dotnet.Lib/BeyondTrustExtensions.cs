using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Microsoft.Extensions.Configuration;

public static class BeyondTrustExtensions
{
    /// <summary>
    /// Java'daki createAndLoad() mantığına eşdeğerdir. 
    /// Ortam değişkenlerini (ConfigMap) otomatik tarar ve kütüphaneyi hazır hale getirir.
    /// </summary>
    public static IConfigurationBuilder AddBeyondTrustSecrets(this IConfigurationBuilder builder)
    {
        // 1. ADIM: Mevcut ortam değişkenlerini geçici olarak derle
        // Bu sayede BEYONDTRUST_ ile başlayan değişkenlere erişebileceğiz
        var tempConfig = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        // 2. ADIM: Models.cs içindeki BeyondTrustOptions sınıfına otomatik map'le
        // Bu işlem Java'daki "fromEnv()" metodunun yaptığı işi yapar.
        var options = new BeyondTrustOptions();
        tempConfig.Bind(options);

        // 3. ADIM: Aktivasyon Kontrolü
        if (options.Enabled)
        {
            if (!string.IsNullOrEmpty(options.ApiKey) && !string.IsNullOrEmpty(options.ApiUrl))
            {
                // Her şey hazır! Provider'ı sisteme dahil et.
                // BeyondTrustConfigurationProvider.Load() metodun burada tetiklenecek.
                builder.Add(new BeyondTrustConfigurationSource(options));
                Console.WriteLine("🚀 [BeyondTrust] Zero-Config aktif. İlk veriler çekiliyor...");
            }
            else
            {
                Console.WriteLine("⚠️ [BeyondTrust] Kütüphane aktif (Enabled=true) fakat API_KEY veya URL eksik.");
            }
        }
        else
        {
            Console.WriteLine("ℹ️ [BeyondTrust] Kütüphane devre dışı (Enabled=false).");
        }

        return builder;
    }
}