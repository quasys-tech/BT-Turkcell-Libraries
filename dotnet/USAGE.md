# .NET Usage

## Library Ekleme

Package reference örneği:

```bash
dotnet add package Turkcell.BT.Dotnet.Lib
```

Bu repo içinde `ProjectReference` örneği:

```xml
<ProjectReference Include="..\src\Turkcell.BT.Dotnet.Lib\Turkcell.BT.Dotnet.Lib.csproj" />
```

## Minimal Code

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.LinuxProd.root"];
var safePassword = configuration["bt.safe.Team/Db.AppDb.password"];
```

## Local Windows

`classic API auth` örneği:

```powershell
. .\examples\env\windows-apikey.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

`OAuth` örneği:

```powershell
. .\examples\env\windows-oauth.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

## Local Linux

`classic API auth` örneği:

```bash
source ./examples/env/linux-apikey.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

`OAuth` örneği:

```bash
source ./examples/env/linux-oauth.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

## Kubernetes

Önerilen manifest setleri:

- `classic API auth`: [k8s/apikey-configmap.yml](k8s/apikey-configmap.yml), [k8s/apikey-secret.yml](k8s/apikey-secret.yml), [k8s/apikey-deployment.yml](k8s/apikey-deployment.yml)
- `OAuth`: [k8s/oauth-configmap.yml](k8s/oauth-configmap.yml), [k8s/oauth-secret.yml](k8s/oauth-secret.yml), [k8s/oauth-deployment.yml](k8s/oauth-deployment.yml)

## Demo App

Demo app:

- sadece environment variable okur
- iki auth mode'u da destekler
- `BEYONDTRUST_USE_APP_USER` değerinin explicit verilmesini bekler
- yüklenen tüm `bt.*` key'lerini yazdırır
- seçilen example managed account, Secret Safe password ve Secret Safe username key'lerini raw loglar

Çalıştırma komutu:

```bash
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

Demo-only helper parameter örnekleri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`
- Bir helper parameter set edilmemişse demo app ilgili example output için skip mesajı yazar.
- Bir helper parameter yüklenmiş bir key'e işaret etmiyorsa demo app `Demo example key not found: <key>` mesajı yazar.

## OAuth Senaryosu

Gerekli parameter'lar:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true`
- `BEYONDTRUST_CLIENT_ID=<CLIENT_ID>`
- `BEYONDTRUST_CLIENT_SECRET=<CLIENT_SECRET>`

## classic API auth Senaryosu

Gerekli parameter'lar:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=false`
- `BEYONDTRUST_API_KEY=<API_KEY>` veya `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;`
- `BEYONDTRUST_RUNAS_USER=<RUNAS_USER>` değerini `runas` bilgisini ayrı vermek istiyorsanız kullanın

## Refresh Interval Notu

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` sadece backward compatibility için desteklenen legacy alias'tır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
