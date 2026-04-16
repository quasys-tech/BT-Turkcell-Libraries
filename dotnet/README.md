# Turkcell.BT.Dotnet.Lib

`Turkcell.BT.Dotnet.Lib`, BeyondTrust managed account password ve Secret Safe value'larini `IConfiguration` icine yukler.

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Uretilen Key Formatlari

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

- `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_ENABLED=true` oldugunda explicit olarak verilmelidir.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir.
- `BT_REFRESH_TIME` legacy alias olarak desteklenir. Canonical parameter yoksa ve parse edilebiliyorsa kullanilir.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error olusur. Bu durumda `BT_REFRESH_TIME` veya default value'ya silent fallback yoktur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa default value kullanilir.
- Shared boolean parameter'lar invalid ise validation error olusur.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` backward compatibility icin kabul edilir, fakat Secret Safe yuklemesi yine `BEYONDTRUST_SECRET_SAFE_PATHS` ile path-based calisir.
- Demo app raw secret logging yaptigi icin ayni logging style production kullanimda onerilmez.
- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardir.

## Diger Docs

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- Package-level notlar: [src/Turkcell.BT.Dotnet.Lib/README.md](src/Turkcell.BT.Dotnet.Lib/README.md)
