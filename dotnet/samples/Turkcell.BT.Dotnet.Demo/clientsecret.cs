// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Turkcell.BT.Dotnet.Lib; // Kütüphanen burası

// Console.WriteLine("====================================================");
// Console.WriteLine("🚀 TURKCELL BEYONDTRUST PAM LIBRARY - OAUTH LIVE DEMO");
// Console.WriteLine("====================================================");

// // 1. Manuel Test Ortamı Ayarları (OAuth/App User için güncellendi)
// SetEnvironmentVariables();

// // 2. Uygulama Yapılandırması
// var builder = Host.CreateApplicationBuilder(args);

// // --- Kütüphaneyi Takıyoruz ---
// builder.Configuration.AddBeyondTrustSecrets(); 
// // -----------------------------

// var host = builder.Build();
// var config = host.Services.GetRequiredService<IConfiguration>();

// Console.WriteLine("\n🚀 Uygulama Başladı. Şifreler OAuth (AppUser) ile izleniyor...");
// // Refresh süresini env'den okuduğumuzu teyit edelim (default 20 değil artık 5 olacak)
// var refreshTime = Environment.GetEnvironmentVariable("BEYONDTRUST_REFRESH_INTERVAL") ?? "Unknown";
// Console.WriteLine($"ℹ️  Refresh süresi: {refreshTime} saniye.\n");

// // Başlangıçta yüklü keyleri görelim
// PrintAllBeyondTrustKeys(config);

// // 3. İZLEME DÖNGÜSÜ
// var lastDbPass = "";
// var lastApiPass = "";

// while (true)
// {
//     // config[...] üzerinden her zaman en taze veriyi çekiyoruz
//     var currentDbPass = config["bt.acc.EC2AMAZ-D6OKDG1.deneme"] ?? "YOK";
//     var currentApiPass = config["bt.safe.ENES_SC_DEMO_DEV.testtypesecret1.password"] ?? "YOK";

//     if (currentDbPass != lastDbPass || currentApiPass != lastApiPass)
//     {
//         Console.WriteLine($"\n🔄 [{DateTime.Now:HH:mm:ss}] DEĞİŞİKLİK VEYA İLK YÜKLEME ALGILANDI!");
//         Console.WriteLine($"   📦 DB Pass : {currentDbPass}");
//         Console.WriteLine($"   📦 API Pass: {currentApiPass}");

//         lastDbPass = currentDbPass;
//         lastApiPass = currentApiPass;
//     }
//     else
//     {
//         Console.Write("."); // Yaşadığını belirtmek için nokta basar
//     }

//     await Task.Delay(2000); 
// }

// void PrintAllBeyondTrustKeys(IConfiguration configuration)
// {
//     Console.WriteLine("\n--- 🛡️  BEYONDTRUST LOADED KEYS ---");
//     var btKeys = configuration.AsEnumerable()
//         .Where(x => x.Key.StartsWith("bt.", StringComparison.OrdinalIgnoreCase))
//         .OrderBy(x => x.Key);

//     foreach (var kvp in btKeys)
//     {
//         Console.WriteLine($"🔑 {kvp.Key} = {kvp.Value}");
//     }
//     Console.WriteLine("----------------------------------\n");
// }

// void SetEnvironmentVariables()
// {
//     // --- OAUTH (APP USER) AYARLARI ---
//     Environment.SetEnvironmentVariable("BEYONDTRUST_USE_APP_USER", "true"); // Yeni modu aktif et
//     Environment.SetEnvironmentVariable("BEYONDTRUST_CLIENT_ID", "3de4ceb1-bd32-4088-816b-c23eff735d24");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_CLIENT_SECRET", "AOsW+TtZsfx3IvRr0vtYJnnSwDldSv+l1GjZ5jQf03o=");
    
//     // --- GENEL AYARLAR ---
//     Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.quasys.com.tr/BeyondTrust/api/public/v3");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_REFRESH_INTERVAL", "5"); // Hızlı test için 5 saniye
//     Environment.SetEnvironmentVariable("BEYONDTRUST_IGNORE_SSL_ERRORS", "true");

//     // --- HEDEF HESAPLAR VE SAFE'LER ---
//     Environment.SetEnvironmentVariable("BEYONDTRUST_SECRET_SAFE_PATHS", "ENES_SC_DEMO_DEV,ENES_SC_DEMO_TEST");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_MANAGED_ACCOUNTS", "dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB;EC2AMAZ-D6OKDG1.deneme");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "true");

//     // Not: API Key artık zorunlu değil, o yüzden sildim/yorum satırı yaptım.
//     // Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", "..."); 
// }