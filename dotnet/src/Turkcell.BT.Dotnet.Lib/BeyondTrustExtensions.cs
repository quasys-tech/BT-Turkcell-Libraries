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

        // 3. ADIM: Aktivasyon ve Validasyon Kontrolü
        if (options.Enabled)
        {
            bool hasValidUrl = !string.IsNullOrWhiteSpace(options.ApiUrl);
            
            // Senaryo 1: OAuth (App User) Kullanımı
            bool isOAuthReady = options.UseAppUser 
                                && !string.IsNullOrWhiteSpace(options.ClientId) 
                                && !string.IsNullOrWhiteSpace(options.ClientSecret);

            // Senaryo 2: Klasik API Key Kullanımı (UseAppUser false ise buna bakar)
            bool isApiKeyReady = !options.UseAppUser 
                                 && !string.IsNullOrWhiteSpace(options.ApiKey);

            // URL geçerli mi VE (OAuth hazır mı VEYA ApiKey hazır mı?)
            if (hasValidUrl && (isOAuthReady || isApiKeyReady))
            {
                // Her şey hazır! Provider'ı sisteme dahil et.
                builder.Add(new BeyondTrustConfigurationSource(options));

                string authMode = options.UseAppUser ? "OAuth2 (App User)" : "Legacy API Key";
                Console.WriteLine($"🚀 [BeyondTrust] Zero-Config aktif. Auth Modu: {authMode}");
                Console.WriteLine("ℹ️  İlk veriler çekiliyor...");
            }
            else
            {
                Console.WriteLine("⚠️ [BeyondTrust] Kütüphane aktif (Enabled=true) fakat konfigürasyon eksik.");
                if (!hasValidUrl) Console.WriteLine("   -> Eksik: BEYONDTRUST_API_URL");
                
                if (options.UseAppUser)
                {
                    if (string.IsNullOrWhiteSpace(options.ClientId)) Console.WriteLine("   -> Eksik: BEYONDTRUST_CLIENT_ID (AppUser modu açık)");
                    if (string.IsNullOrWhiteSpace(options.ClientSecret)) Console.WriteLine("   -> Eksik: BEYONDTRUST_CLIENT_SECRET (AppUser modu açık)");
                }
                else if (string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    Console.WriteLine("   -> Eksik: BEYONDTRUST_API_KEY");
                }
            }
        }
        else
        {
            Console.WriteLine("ℹ️ [BeyondTrust] Kütüphane devre dışı (Enabled=false).");
        }

        return builder;
    }
}