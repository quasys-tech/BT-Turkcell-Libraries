// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Turkcell.BT.Dotnet.Lib;

// Console.WriteLine("====================================================");
// Console.WriteLine("🚀 TURKCELL BEYONDTRUST .NET POC (Proof of Concept)");
// Console.WriteLine("====================================================");

// var builder = Host.CreateApplicationBuilder(args);

// // ⭐ SIHIRLI SATIR: Environment değişkenlerini otomatik okur ve servisi bağlar.
// builder.Configuration.AddBeyondTrustSecrets(); 

// var host = builder.Build();
// var config = host.Services.GetRequiredService<IConfiguration>();

// // ConfigMap'ten izlenecek örnek keyleri alıyoruz
// var targetAccKey  = Environment.GetEnvironmentVariable("BT_EXAMPLE_ACCOUNT") ?? "bt.acc.default.account";
// var targetSafeKey = Environment.GetEnvironmentVariable("BT_EXAMPLE_SAFE_PASSWORD") ?? "bt.safe.default.password";

// Console.WriteLine($"🔍 İzlenen Account: {targetAccKey}");
// Console.WriteLine($"🔍 İzlenen Safe   : {targetSafeKey}\n");

// string? lastAccValue = null;
// string? lastSafeValue = null;

// while (true)
// {
//     var currentAccValue  = config[targetAccKey] ?? "YOK";
//     var currentSafeValue = config[targetSafeKey] ?? "YOK";

//     if (currentAccValue != lastAccValue || currentSafeValue != lastSafeValue)
//     {
//         Console.WriteLine($"\n🔔 [{DateTime.Now:HH:mm:ss}] VERİ GÜNCELLENDİ!");
//         Console.WriteLine($" 🛡️  Account: {currentAccValue}");
//         Console.WriteLine($" 🔑 Safe   : {currentSafeValue}");
//         Console.WriteLine("--------------------------------------------------");

//         lastAccValue = currentAccValue;
//         lastSafeValue = currentSafeValue;
//     }
//     else
//     {
//         Console.Write("."); 
//     }

//     await Task.Delay(5000); 
// }