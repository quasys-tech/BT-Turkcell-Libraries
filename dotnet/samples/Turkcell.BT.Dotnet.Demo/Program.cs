// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Turkcell.BT.Dotnet.Lib; // Kütüphanen burası

// Console.WriteLine("====================================================");
// Console.WriteLine("🚀 TURKCELL BEYONDTRUST PAM LIBRARY - LIVE DEMO");
// Console.WriteLine("====================================================");

// // 1. Manuel Test Ortamı (Müşteride burası env veya appsettings olacak)
// SetEnvironmentVariables();

// // 2. Uygulama Yapılandırması
// var builder = Host.CreateApplicationBuilder(args);

// // --- Kütüphaneyi Takıyoruz ---
// builder.Configuration.AddBeyondTrustSecrets(); 
// // -----------------------------

// var host = builder.Build();
// var config = host.Services.GetRequiredService<IConfiguration>();

// Console.WriteLine("🚀 Uygulama Başladı. Şifreler izleniyor...");
// Console.WriteLine("ℹ️  Refresh süresi: 20 saniye.\n");

// // Başlangıçta yüklü keyleri görelim
// PrintAllBeyondTrustKeys(config);

// // 3. İZLEME DÖNGÜSÜ
// var lastDbPass = "";
// var lastApiPass = "";

// while (true)
// {
//     // config[...] üzerinden her zaman en taze veriyi çekiyoruz
//     //var currentDbPass = config["bt.acc.dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB"] ?? "YOK";
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
//     Environment.SetEnvironmentVariable("BEYONDTRUST_REFRESH_INTERVAL", "20"); 
//     Environment.SetEnvironmentVariable("BEYONDTRUST_API_URL", "https://pam.quasys.com.tr/BeyondTrust/api/public/v3");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_API_KEY", "b26a593fdf632aa951d69004f8531d99b5bc53c06c83607ef9d09f711d55a9221890a10cce3ad17af906f389424a6a07028be31fcabf4d1a00dfa21fef72f2f4; runas=enes;");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_ENABLED", "true");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_SECRET_SAFE_PATHS", "ENES_SC_DEMO_DEV,ENES_SC_DEMO_TEST");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_MANAGED_ACCOUNTS", "dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB;EC2AMAZ-D6OKDG1.deneme");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "true");
//     Environment.SetEnvironmentVariable("BEYONDTRUST_IGNORE_SSL_ERRORS", "true");
// }