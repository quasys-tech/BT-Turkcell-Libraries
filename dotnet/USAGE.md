# .NET Usage

## Library Ekleme

Package reference ornegi:

```bash
dotnet add package Turkcell.BT.Dotnet.Lib
```

Bu repo icinde `ProjectReference` ornegi:

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

`classic API auth` ornegi:

```powershell
. .\examples\env\windows-apikey.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

`OAuth` ornegi:

```powershell
. .\examples\env\windows-oauth.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

## Local Linux

`classic API auth` ornegi:

```bash
source ./examples/env/linux-apikey.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

`OAuth` ornegi:

```bash
source ./examples/env/linux-oauth.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

## Kubernetes

Onerilen manifest setleri:

- `classic API auth`: [k8s/apikey-configmap.yml](k8s/apikey-configmap.yml), [k8s/apikey-secret.yml](k8s/apikey-secret.yml), [k8s/apikey-deployment.yml](k8s/apikey-deployment.yml)
- `OAuth`: [k8s/oauth-configmap.yml](k8s/oauth-configmap.yml), [k8s/oauth-secret.yml](k8s/oauth-secret.yml), [k8s/oauth-deployment.yml](k8s/oauth-deployment.yml)

## Demo App

Demo app:

- sadece environment variable okur
- iki auth mode'u da destekler
- `BEYONDTRUST_USE_APP_USER` degerinin explicit verilmesini bekler
- yuklenen tum `bt.*` key'lerini yazdirir
- secilen example managed account, Secret Safe password ve Secret Safe username key'lerini raw loglar

Calistirma komutu:

```bash
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

Demo-only helper parameter ornekleri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`
- Bir helper parameter set edilmemisse demo app ilgili example output icin skip mesaji yazar.
- Bir helper parameter yuklenmis bir key'e isaret etmiyorsa demo app `Demo example key not found: <key>` mesaji yazar.

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
- `BEYONDTRUST_RUNAS_USER=<RUNAS_USER>` degerini `runas` bilgisini ayri vermek istiyorsaniz kullanin

## Refresh Interval Notu

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir.
- `BT_REFRESH_TIME` sadece backward compatibility icin desteklenen legacy alias'tir.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error olusur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa default value kullanilir.
