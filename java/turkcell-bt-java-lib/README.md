# com.turkcell.bt.java

`com.turkcell.bt.java`, BeyondTrust managed account password ve Secret Safe value'larını refresh edilebilir in-memory configuration manager içine yükler.

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Üretilen Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Quick Integration

```java
try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String managedPassword = manager.getProperty("bt.acc.LinuxProd.root");
    String secretPassword = manager.getProperty("bt.safe.Team/Db.AppDb.password");
}
```

## Notlar

- `Java` ve `.NET` tarafında aynı shared parameter set'inin aynı davranışı üretmesi hedeflenir.
- `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_ENABLED=true` olduğunda explicit olarak verilmelidir.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` legacy alias olarak desteklenir. Canonical parameter yoksa ve parse edilebiliyorsa kullanılır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur. Bu durumda legacy alias veya `default value`'ya silent fallback yoktur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
- Shared boolean parameter'lar invalid ise validation error oluşur.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` compatibility için kabul edilir, fakat Secret Safe yüklemesi yine `BEYONDTRUST_SECRET_SAFE_PATHS` ile path-based çalışır.
- Demo app raw secret logging yaptığı için aynı logging style production kullanımda önerilmez.
- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardır.

## Diğer Docs

- [../USAGE.md](../USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
