# Turkcell.BT.Dotnet.Lib

`Turkcell.BT.Dotnet.Lib`, BeyondTrust managed account ve Secret Safe değerlerini `.NET IConfiguration` içine yükler.

## Artifactory'den Ekleme

NuGet source örneği:

```bash
dotnet nuget add source "https://<ARTIFACTORY_HOST>/artifactory/api/nuget/<NUGET_REPO_KEY>/v3/index.json" --name bt-artifactory
```

Package ekleme örneği:

```bash
dotnet add package Turkcell.BT.Dotnet.Lib --version <VERSION> --source bt-artifactory
```

`PackageReference` örneği:

```xml
<PackageReference Include="Turkcell.BT.Dotnet.Lib" Version="<VERSION>" />
```

## Minimal Entegrasyon

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.MySystem.MyAccount"];
var secretPassword = configuration["bt.safe.MyFolder.MyTitle.password"];
var secretUsername = configuration["bt.safe.MyFolder.MyTitle.username"];
```

## Gerekli Konfigürasyon

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true` veya `false`
- `OAuth` için: `BEYONDTRUST_CLIENT_ID` ve `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için: `BEYONDTRUST_API_KEY` ve gerekirse `BEYONDTRUST_RUNAS_USER`
- Yüklenecek hedefler için: `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`
- Opsiyonel refresh ayarı için: `BEYONDTRUST_REFRESH_INTERVAL`

## Üretilen Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Notlar

- `BEYONDTRUST_USE_APP_USER` değeri explicit verilmelidir.
- Library başlangıçta snapshot yükler, refresh aktifse arka planda günceller.
- Normal kullanımda per-refresh başarı logu basmaz. Detaylı log gerekiyorsa `BEYONDTRUST_DEBUG=true` kullanılabilir.
- Demo doğrulaması için sample proje içindeki `POC` entrypoint kullanılabilir.

## Diğer Docs

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- Package notları: [src/Turkcell.BT.Dotnet.Lib/README.md](src/Turkcell.BT.Dotnet.Lib/README.md)
