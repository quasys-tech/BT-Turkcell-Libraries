# com.turkcell.bt.java

`com.turkcell.bt.java`, BeyondTrust managed account password ve Secret Safe value'larini refresh edilebilir in-memory configuration manager icine yukler.

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Uretilen Key Formatlari

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

- `Java` ve `.NET` tarafinda ayni shared parameter set'inin ayni davranisi uretmesi hedeflenir.
- `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_ENABLED=true` oldugunda explicit olarak verilmelidir.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir.
- `BT_REFRESH_TIME` legacy alias olarak desteklenir. Canonical parameter yoksa ve parse edilebiliyorsa kullanilir.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error olusur. Bu durumda legacy alias veya default value'ya silent fallback yoktur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa default value kullanilir.
- Shared boolean parameter'lar invalid ise validation error olusur.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` compatibility icin kabul edilir, fakat Secret Safe yuklemesi yine `BEYONDTRUST_SECRET_SAFE_PATHS` ile path-based calisir.
- Demo app raw secret logging yaptigi icin ayni logging style production kullanimda onerilmez.
- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardir.

## Diger Docs

- [../USAGE.md](../USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
