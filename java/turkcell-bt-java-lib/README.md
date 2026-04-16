# com.turkcell.bt.java

`com.turkcell.bt.java` loads BeyondTrust managed account passwords and Secret Safe values into a refreshable in-memory configuration manager.

Supported auth modes:

- OAuth / App User / Client Credentials
- Classic API authentication

Produced key formats:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

Quick integration:

```java
try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String managedPassword = manager.getProperty("bt.acc.LinuxProd.root");
    String secretPassword = manager.getProperty("bt.safe.Team/Db.AppDb.password");
}
```

Notes:

- Same shared parameter set is expected to produce the same behavior in `Java` and `.NET`.
- `BEYONDTRUST_USE_APP_USER` must be explicitly set to `true` or `false` whenever `BEYONDTRUST_ENABLED=true`.
- `BEYONDTRUST_REFRESH_INTERVAL` is the canonical refresh parameter.
- `BT_REFRESH_TIME` is accepted only as a backward-compatible alias when `BEYONDTRUST_REFRESH_INTERVAL` is absent and parseable.
- An invalid `BEYONDTRUST_REFRESH_INTERVAL` value is a configuration error. An invalid `BT_REFRESH_TIME` value falls back to the default refresh interval only when the canonical setting is absent.
- Invalid boolean values for shared boolean parameters are configuration errors.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` is accepted for compatibility, but Secret Safe loading still uses `BEYONDTRUST_SECRET_SAFE_PATHS`.
- Demo applications intentionally print raw secret values. Do not copy that logging style into production code.
- Demo-only helper parameters choose which raw demo values are highlighted:
  `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
  `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
  `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`

More docs:

- [../USAGE.md](../USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
