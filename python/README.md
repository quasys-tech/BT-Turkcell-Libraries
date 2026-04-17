# turkcell-bt-python-lib

`turkcell-bt-python-lib`, BeyondTrust managed account password ve Secret Safe value'larını refresh edilebilir in-memory configuration manager içine yükler.

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Üretilen Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Quick Start

```python
from turkcell_bt_python.configuration_manager import BeyondTrustConfigurationManager

with BeyondTrustConfigurationManager.create_and_load() as manager:
    managed_password = manager.get_property("bt.acc.LinuxProd.root")
    secret_password = manager.get_property("bt.safe.Team/Db.AppDb.password")
```

## Notlar

- `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_ENABLED=true` olduğunda explicit olarak verilmelidir.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` legacy alias olarak desteklenir. Canonical parameter yoksa ve parse edilebiliyorsa kullanılır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur. Bu durumda `BT_REFRESH_TIME` veya `default value`'ya silent fallback yoktur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
- Shared boolean parameter'lar invalid ise validation error oluşur.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` compatibility için kabul edilir, fakat Secret Safe yüklemesi yine `BEYONDTRUST_SECRET_SAFE_PATHS` ile path-based çalışır.
- Demo app raw secret logging yaptığı için aynı logging style production kullanımda önerilmez.
- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardır.

## Diğer Docs

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
