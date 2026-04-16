# Turkcell.BT.Dotnet.Lib

`Turkcell.BT.Dotnet.Lib`, BeyondTrust managed account password ve Secret Safe value'larını `IConfiguration` içine yükler.

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Üretilen Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Quick Integration

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.LinuxProd.root"];
var secretPassword = configuration["bt.safe.Team/Db.AppDb.password"];
```

## Notlar

- `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_ENABLED=true` olduğunda explicit olarak verilmelidir.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` legacy alias olarak desteklenir. Canonical parameter yoksa ve parse edilebiliyorsa kullanılır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur. Bu durumda `BT_REFRESH_TIME` veya `default value`'ya silent fallback yoktur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
- Shared boolean parameter'lar invalid ise validation error oluşur.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` backward compatibility için kabul edilir, fakat Secret Safe yüklemesi yine `BEYONDTRUST_SECRET_SAFE_PATHS` ile path-based çalışır.
- Demo app raw secret logging yaptığı için aynı logging style production kullanımda önerilmez.
- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardır.

## Diğer Docs

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- Package-level notlar: [src/Turkcell.BT.Dotnet.Lib/README.md](src/Turkcell.BT.Dotnet.Lib/README.md)
