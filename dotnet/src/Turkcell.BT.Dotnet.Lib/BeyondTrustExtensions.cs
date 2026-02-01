using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// IConfigurationBuilder için BeyondTrust entegrasyonu sağlayan genişletme metodları.
/// </summary>
public static class BeyondTrustExtensions
{
    /// <summary>
    /// Turkcell BeyondTrust Secret Management sistemini konfigürasyon kaynaklarına ekler.
    /// Environment değişkenlerinden veya mevcut konfigürasyondan BEYONDTRUST_ ayarlarını okur.
    /// </summary>
    /// <param name="builder">Konfigürasyon oluşturucu</param>
    /// <returns>Güncellenmiş konfigürasyon oluşturucu</returns>
    public static IConfigurationBuilder AddBeyondTrustSecrets(this IConfigurationBuilder builder)
    {
        // Mevcut konfigürasyonu (Environment, appsettings vb.) geçici olarak derleyip ayarları okuyoruz
        var tempConfig = builder.Build();
        var options = tempConfig.Get<BeyondTrustOptions>() ?? new BeyondTrustOptions();

        // Kütüphane aktifse ve API anahtarı tanımlıysa kaynağı ekle
        if (options.Enabled)
        {
            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                builder.Add(new BeyondTrustConfigurationSource(options));
            }
            else
            {
                // Kritik: Aktif edilmek istenmiş ama API Key girilmemişse log basılır
                Console.WriteLine("⚠️ [Turkcell.BT.BeyondTrust] Library is ENABLED but BEYONDTRUST_API_KEY is missing. Skipping source.");
            }
        }

        return builder;
    }
}