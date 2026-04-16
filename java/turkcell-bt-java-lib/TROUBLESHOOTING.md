# Java Troubleshooting

## Auth Failed

- `BEYONDTRUST_USE_APP_USER` değerini kontrol edin.
- `BEYONDTRUST_ENABLED=true` ise `BEYONDTRUST_USE_APP_USER` explicit verilmelidir.
- `BEYONDTRUST_ENABLED`, `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_IGNORE_SSL_ERRORS`, `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` ve `BEYONDTRUST_ALL_SECRETS_ENABLED` invalid boolean value alırsa validation error oluşur.
- `OAuth` kullanıyorsanız `BEYONDTRUST_CLIENT_ID` ve `BEYONDTRUST_CLIENT_SECRET` değerlerini kontrol edin.
- `classic API auth` kullanıyorsanız `BEYONDTRUST_API_KEY` ve gerekliyse `BEYONDTRUST_RUNAS_USER` değerlerini kontrol edin.

## API URL Yanlış

- `BEYONDTRUST_API_URL`, BeyondTrust public API base URL değerine işaret etmelidir.
- Beklenen endpoint ailesi `Auth/Connect/Token`, `Auth/SignAppin`, `ManagedAccounts`, `Requests`, `Credentials/{id}`, `Requests/{id}/Checkin`, `Secrets-Safe/Secrets` endpoint'lerini içerir.

## SSL/TLS Error

- Production için `BEYONDTRUST_IGNORE_SSL_ERRORS=false` kullanın.
- Private CA kullanan bir endpoint'e bağlanıyorsanız `BEYONDTRUST_CERTIFICATE_CONTENT` verin.
- `BEYONDTRUST_IGNORE_SSL_ERRORS=true` sadece demo veya kontrollü dev ortamı için uygundur.

## Secret Gelmiyor

- Key kullanımını birebir kontrol edin.
- Managed account key format'ı `bt.acc.{SystemName}.{AccountName}` şeklindedir.
- Secret Safe key format'ları `bt.safe.{Folder}.{Title}.password` ve `bt.safe.{Folder}.{Title}.username` şeklindedir.

## Managed Account Bulunamadı

- `BEYONDTRUST_MANAGED_ACCOUNTS` değerinin BeyondTrust tarafındaki `SystemName.AccountName` ile birebir eşleştiğini kontrol edin.
- Tüm erişilebilir account'ları yüklemek istiyorsanız `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=true` kullanın.

## Secret Safe Path Yanlış

- `BEYONDTRUST_SECRET_SAFE_PATHS` değerini kontrol edin.
- Bu sürümde `BEYONDTRUST_ALL_SECRETS_ENABLED` global enumerate davranışı başlatmaz.

## Refresh Çalışmıyor

- `BEYONDTRUST_REFRESH_INTERVAL=0` background refresh'i disabled yapar.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır ve iki refresh parameter birlikte verilirse her zaman kazanır.
- `BT_REFRESH_TIME`, canonical parameter yoksa kullanılan legacy alias'tır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
- Refresh sırasında bir hata olursa library son başarılı snapshot'ı korur.

## system property ve environment variable Sırası

- Resolution sırası `system property -> environment variable -> default value` şeklindedir.
- Local demo app çalıştırırken `-D...` ile verilen `system property` değerleri shell seviyesindeki environment variable değerlerinden önce okunur.

## Demo Example Output Gelmiyor

- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardır.
- Bu parameter'ları gerçek bir `bt.*` key'ine işaret edecek şekilde set edin.
- Helper parameter set edilmemişse demo app ilgili example output için skip mesajı yazar.
- Helper parameter var ama key yüklenmemişse demo app `Demo example key not found: <key>` mesajı yazar.
