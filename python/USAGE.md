# Python Usage

## Önerilen Entegrasyon Akışı

1. Package'i Artifactory üzerindeki PyPI repository'sinden kurun.
2. Uygulama başlangıcında BeyondTrust environment variable'larını verin.
3. `BeyondTrustConfigurationManager.create_and_load()` ile manager'ı oluşturun.
4. Uygulama içinde canonical `bt.*` key'leri üzerinden değerleri okuyun.

## Artifactory Örneği

```bash
python -m pip install --index-url "https://<ARTIFACTORY_HOST>/artifactory/api/pypi/<PYPI_REPO_KEY>/simple" turkcell-bt-python-lib==<VERSION>
```

## Minimal Kod

```python
from turkcell_bt_python.configuration_manager import BeyondTrustConfigurationManager

with BeyondTrustConfigurationManager.create_and_load() as manager:
    managed_password = manager.get_property("bt.acc.MySystem.MyAccount")
    safe_password = manager.get_property("bt.safe.MyFolder.MyTitle.password")
    safe_username = manager.get_property("bt.safe.MyFolder.MyTitle.username")
```

## Zorunlu Ayarlar

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true` veya `false`
- `OAuth` için: `BEYONDTRUST_CLIENT_ID`, `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için: `BEYONDTRUST_API_KEY`, opsiyonel `BEYONDTRUST_RUNAS_USER`
- Yüklenecek hedefler için: `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`

## POC ile Hızlı Doğrulama

`POC`, sadece seçilen 3 örnek key'i yazar, refresh açıksa süreç açık kalır ve çıktı değiştiğinde blok halinde tekrar basar.

Classic API:

```powershell
. .\examples\env\windows-apikey.ps1.sample
turkcell-bt-poc
```

OAuth:

```powershell
. .\examples\env\windows-oauth.ps1.sample
turkcell-bt-poc
```

Alternatif çalıştırma:

```bash
python -m turkcell_bt_python.poc
```

Demo helper key'leri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.MySystem.MyAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.MyFolder.MyTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.MyFolder.MyTitle.username`

## Kubernetes

Önerilen manifest setleri:

- `Classic API`: [k8s/apikey-configmap.yml](k8s/apikey-configmap.yml), [k8s/apikey-secret.yml](k8s/apikey-secret.yml), [k8s/apikey-deployment.yml](k8s/apikey-deployment.yml)
- `OAuth`: [k8s/oauth-configmap.yml](k8s/oauth-configmap.yml), [k8s/oauth-secret.yml](k8s/oauth-secret.yml), [k8s/oauth-deployment.yml](k8s/oauth-deployment.yml)

## Operasyon Notları

- Normal kullanımda per-refresh başarı logu basılmaz.
- `Classic API` modunda `Auth/SignAppin` adımı başarısız olsa bile library veri yüklemeye devam etmeyi dener.
- Proxy environment variable'larından etkilenmemesi için session `trust_env=false` ile çalışır.
- Daha detaylı log gerekiyorsa geçici olarak `BEYONDTRUST_DEBUG=true` kullanılabilir.
- Refresh ayarı için canonical parametre `BEYONDTRUST_REFRESH_INTERVAL`'dır.
