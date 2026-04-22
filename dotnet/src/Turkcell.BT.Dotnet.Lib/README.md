# Turkcell.BT.Dotnet.Lib Package Notes

`Turkcell.BT.Dotnet.Lib`, BeyondTrust değerlerini `.NET IConfiguration` içine ekler.

## Package Reference

```xml
<PackageReference Include="Turkcell.BT.Dotnet.Lib" Version="<VERSION>" />
```

## Minimal Kullanım

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.MySystem.MyAccount"];
var safePassword = configuration["bt.safe.MyFolder.MyTitle.password"];
var safeUsername = configuration["bt.safe.MyFolder.MyTitle.username"];
```

## Gerekli Ayarlar

- `BEYONDTRUST_API_URL`
- `BEYONDTRUST_USE_APP_USER`
- `OAuth` için `BEYONDTRUST_CLIENT_ID` ve `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için `BEYONDTRUST_API_KEY`, opsiyonel `BEYONDTRUST_RUNAS_USER`
- `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`

## Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`
