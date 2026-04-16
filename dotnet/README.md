# Turkcell.BT.Dotnet.Lib

`Turkcell.BT.Dotnet.Lib` loads BeyondTrust managed account passwords and Secret Safe values into `IConfiguration`.

Supported auth modes:

- OAuth / App User / Client Credentials
- Classic API authentication

Produced key formats:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

Quick integration:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.LinuxProd.root"];
var secretPassword = configuration["bt.safe.Team/Db.AppDb.password"];
```

Notes:

- Same shared parameter set is expected to produce the same behavior in `.NET` and `Java`.
- `BEYONDTRUST_USE_APP_USER` must be explicitly set to `true` or `false` whenever `BEYONDTRUST_ENABLED=true`.
- `BEYONDTRUST_REFRESH_INTERVAL` is the canonical refresh setting.
- `BT_REFRESH_TIME` is a legacy alias that is used only when `BEYONDTRUST_REFRESH_INTERVAL` is absent and parseable.
- An invalid `BEYONDTRUST_REFRESH_INTERVAL` value is a configuration error. An invalid `BT_REFRESH_TIME` value falls back to the default refresh interval only when the canonical setting is absent.
- Invalid boolean values for shared boolean parameters are configuration errors.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` is accepted for backward compatibility, but this version still loads Secret Safe values by `BEYONDTRUST_SECRET_SAFE_PATHS`.
- Demo applications intentionally print raw secret values. Do not copy that logging style into production code.
- Demo-only helper parameters choose which raw demo values are highlighted:
  `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
  `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
  `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`

More docs:

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- Library package notes: [src/Turkcell.BT.Dotnet.Lib/README.md](src/Turkcell.BT.Dotnet.Lib/README.md)
