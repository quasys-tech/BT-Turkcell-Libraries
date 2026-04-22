# Turkcell BT Python Library

`turkcell-bt-python-lib`, BeyondTrust managed account ve Secret Safe değerlerini refresh destekli bir configuration manager içine yükler.

## Artifactory'den Ekleme

Pip install örneği:

```bash
python -m pip install --index-url "https://<ARTIFACTORY_HOST>/artifactory/api/pypi/<PYPI_REPO_KEY>/simple" turkcell-bt-python-lib==<VERSION>
```

Import örneği:

```python
from turkcell_bt_python.configuration_manager import BeyondTrustConfigurationManager

with BeyondTrustConfigurationManager.create_and_load() as manager:
    managed_password = manager.get_property("bt.acc.MySystem.MyAccount")
    secret_password = manager.get_property("bt.safe.MyFolder.MyTitle.password")
    secret_username = manager.get_property("bt.safe.MyFolder.MyTitle.username")
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
- Demo doğrulaması için `turkcell-bt-poc` console script'i kullanılabilir.

## Diğer Docs

- [USAGE.md](USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
