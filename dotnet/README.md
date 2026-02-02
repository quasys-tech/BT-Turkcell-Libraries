# BeyondTrust .NET Library Örnek Kullanımı 🛡️
Bu kütüphane, .NET uygulamalarının BeyondTrust Password Safe üzerindeki Managed Account ve Secret Safe şifrelerini IConfiguration hiyerarşisine otomatik olarak enjekte eder.

🚀 Hızlı Başlangıç (Entegrasyon)
## 1. Bağımlılığı Ekle (NuGet)
Projenizin .csproj dosyasına kütüphaneyi ekleyin (Artifactory entegrasyonu sonrası):
```dotnet

<ItemGroup>
    <PackageReference Include="Turkcell.BT.Dotnet.Lib" Version="1.0.0" />
</ItemGroup>

```

## Kullanım (Kod)

Uygulamanızın başlangıcında (Program.cs) sadece tek bir satır ekleyerek tüm şifreleri konfigürasyona dahil edebilirsini

```java

using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

var builder = Host.CreateApplicationBuilder(args);

// ⭐ SİHİRLİ SATIR: Ortam değişkenlerini (ConfigMap) otomatik okur ve servisi bağlar.
builder.Configuration.AddBeyondTrustSecrets(); 

var host = builder.Build();
var config = host.Services.GetRequiredService<IConfiguration>();

// Kullanım: Standart IConfiguration üzerinden erişim
string dbPass = config["bt.acc.SystemName.AccountName"];
string apiPass = config["bt.safe.FolderName.SecretTitle.password"];

```

## Yapılandırma (OpenShift / Deployment)

Kütüphanenin çalışması için aşağıdaki ortam değişkenlerinin ConfigMap üzerinden pod'a enjekte edilmesi gerekir:


`BEYONDTRUST_API_URL` Beyondtrust API Adresi -- `https://secrets-cache-service/BeyondTrust/api/public/v3`

`BEYONDTRUST_API_KEY` Erişim Key'i  (PS-Auth) -- `BEYONDTRUST_API_KEY=..<ApiKey>.; runas=.<User>..;`

`BT_REFRESH_TIME` Yenileme periyodu (saniye) , `default 1800 . 0 ise yenileme yapmaz`

`BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` yetkili olunan tüm managed account'lar çekilsin mi ? ` true/false `

`BEYONDTRUST_MANAGED_ACCOUNTS` Managed Account'lar (;) ile ayrılır . ManagedSystem.Managed Account key'i ile kour. `System1.Acc1;System2.Acc2`

`BEYONDTRUST_SECRET_SAFE_PATHS` Secret Safe bilgileri , Birden fazla olduğu noktada "," ile ayrılır. `SafeFolder1,SafeFolder2`


## 🔑 Key Formatı Kuralları
Manager üzerinden şifre çağırırken aşağıdaki formatları kullanmalısınız:

Managed Accounts:` bt.acc.[SystemName].[AccountName] `

Secret Safe (Şifre):` bt.safe.[Folder].[Title].password `

Secret Safe (Kullanıcı):` bt.safe.[Folder].[Title].username `


## 🛠️ Sorun Giderme
LOGS: Uygulama başladığında `🚀 [BeyondTrust] Zero-Config aktif. İlk veriler çekiliyor... ` logunu gördüğünüzden emin olun.

SSL Hatası: Eğer `The SSL connection could not be established`  alıyorsanız, `BEYONDTRUST_IGNORE_SSL_ERRORS` değerini `true` yapın veya geçerli bir `BEYONDTRUST_CERTIFICATE_CONTENT` sağlayın.

YOK Değeri: Eğer anahtarlar "YOK" dönüyorsa, ConfigMap'teki key isimleri ile `BEYONDTRUST_MANAGED_ACCOUNTS` içeriğinin eşleştiğinden emin olun.




## Example Configmap 

```dotnet

  BEYONDTRUST_ENABLED: "true"
  BEYONDTRUST_API_URL: "https://pandora.turkcell.com.tr/BeyondTrust/api/public/v3"
  BEYONDTRUST_API_KEY: "b26a593fdf632aa951d69004f8531d99b5bc53c06c83607ef9d09f711d55a9221890a10cce3ad17af906f389424a6a07028be31fcabf4d1a00dfa21fef72f2f4; runas=pandora;"

  # SSL ve Refresh Ayarları
  BEYONDTRUST_IGNORE_SSL_ERRORS: "false"
  BT_REFRESH_TIME: "300" ## saniye cinsindendir

  # Hangi veriler çekilecek?
  BEYONDTRUST_MANAGED_ACCOUNTS: "dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB;EC2AMAZ-D6OKDG1.deneme"
  BEYONDTRUST_SECRET_SAFE_PATHS: "PANDORA_SC_DEMO_DEV,PANDORA_SC_DEMO_TEST"
  BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED: "false"
  BEYONDTRUST_CERTIFICATE_CONTENT: |-
    -----BEGIN CERTIFICATE-----
    MIIGejCCBWKgAwIBAgIQCxP8yr431fBRTbEeSyINlzANBgkqhkiG9w0BAQsFADBg
    MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
    d3cuZGlnaWNlcnQuY29tMR8wHQYDVQQDExZHZW9UcnVzdCBUTFMgUlNBIENBIEcx
    MB4XDTI1MDgwMTAwMDAwMFoXDTI2MDkwMTIzNTk1OVowGjEYMBYGA1UEAwwPKi5x
    dWFzeXMuY29tLnRyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4BWo
    OI6cHZgV3pyvE8upY7Q7QoaIPHBVrdF6osShvYvcFAnstdHVJI/mFYak1JcEcPoA
```


### Example Application 


```dotnet
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Turkcell.BT.Dotnet.Lib;

Console.WriteLine("🚀 Uygulama Başlatılıyor...");

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets(); 

var host = builder.Build();
var config = host.Services.GetRequiredService<IConfiguration>();

// ConfigMap'ten hangi key'leri arayacağımızı okuyoruz
string safePassKey = Environment.GetEnvironmentVariable("BT_EXAMPLE_SAFE_PASSWORD") ?? "bt.safe.default";
string managedAccountKey = Environment.GetEnvironmentVariable("BT_EXAMPLE_ACCOUNT") ?? "bt.acc.default";

while (true)
{
    string examplePass = config[safePassKey] ?? "KEY_TANIMSIZ";
    string exampleAcc  = config[managedAccountKey] ?? "KEY_TANIMSIZ";

    Console.WriteLine($"\n⏰ Zaman: {DateTime.Now:HH:mm:ss}");
    Console.WriteLine($"🔑 Safe Password: {examplePass}");
    Console.WriteLine($"🛡️  Account Pass : {exampleAcc}");
    Console.WriteLine("--------------------------------------------------");

    await Task.Delay(5000); 
}
```