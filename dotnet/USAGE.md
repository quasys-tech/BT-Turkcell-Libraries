# .NET Usage

## Önerilen Entegrasyon Akışı

1. Package'i Artifactory üzerindeki NuGet repository'sinden ekleyin.
2. Uygulama başlangıcında BeyondTrust environment variable'larını verin.
3. `builder.Configuration.AddBeyondTrustSecrets();` çağrısını startup aşamasında yapın.
4. Uygulama içinde canonical `bt.*` key'leri üzerinden değerleri okuyun.

## Artifactory Örneği

```bash
dotnet nuget add source "https://<ARTIFACTORY_HOST>/artifactory/api/nuget/<NUGET_REPO_KEY>/v3/index.json" --name bt-artifactory
dotnet add package Turkcell.BT.Dotnet.Lib --version <VERSION> --source bt-artifactory
```

`PackageReference` örneği:

```xml
<PackageReference Include="Turkcell.BT.Dotnet.Lib" Version="<VERSION>" />
```

## Minimal Kod

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.MySystem.MyAccount"];
var safePassword = configuration["bt.safe.MyFolder.MyTitle.password"];
var safeUsername = configuration["bt.safe.MyFolder.MyTitle.username"];
```

## Zorunlu Ayarlar

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true` veya `false`
- `OAuth` için: `BEYONDTRUST_CLIENT_ID`, `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için: `BEYONDTRUST_API_KEY`, opsiyonel `BEYONDTRUST_RUNAS_USER`
- Yüklenecek hedefler için: `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`

## POC ile Hızlı Doğrulama

`POC`, sadece seçilen 3 örnek key'i yazar, refresh açıksa süreç açık kalır ve çıktı değiştiğinde blok halinde tekrar basar.

Classic API:

```powershell
. .\examples\env\windows-apikey.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

OAuth:

```powershell
. .\examples\env\windows-oauth.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

Demo helper key'leri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.MySystem.MyAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.MyFolder.MyTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.MyFolder.MyTitle.username`

## Kubernetes

Önerilen manifest setleri:

- `Classic API`: [k8s/apikey-configmap.yml](k8s/apikey-configmap.yml), [k8s/apikey-secret.yml](k8s/apikey-secret.yml), [k8s/apikey-deployment.yml](k8s/apikey-deployment.yml)
- `OAuth`: [k8s/oauth-configmap.yml](k8s/oauth-configmap.yml), [k8s/oauth-secret.yml](k8s/oauth-secret.yml), [k8s/oauth-deployment.yml](k8s/oauth-deployment.yml)

## Operasyon Notları

- Normal kullanımda per-refresh başarı logu basılmaz.
- `Classic API` modunda `Auth/SignAppin` adımı başarısız olsa bile library veri yüklemeye devam etmeyi dener.
- Daha detaylı log gerekiyorsa geçici olarak `BEYONDTRUST_DEBUG=true` kullanılabilir.
- Refresh ayarı için canonical parametre `BEYONDTRUST_REFRESH_INTERVAL`'dır.
