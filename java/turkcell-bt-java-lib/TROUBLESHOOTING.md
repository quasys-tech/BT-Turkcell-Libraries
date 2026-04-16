# Java Troubleshooting

## Auth Failed

- `BEYONDTRUST_USE_APP_USER` degerini kontrol edin.
- `BEYONDTRUST_ENABLED=true` ise `BEYONDTRUST_USE_APP_USER` explicit verilmelidir.
- `BEYONDTRUST_ENABLED`, `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_IGNORE_SSL_ERRORS`, `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` ve `BEYONDTRUST_ALL_SECRETS_ENABLED` invalid boolean value alirsa validation error olusur.
- `OAuth` kullaniyorsaniz `BEYONDTRUST_CLIENT_ID` ve `BEYONDTRUST_CLIENT_SECRET` degerlerini kontrol edin.
- `classic API auth` kullaniyorsaniz `BEYONDTRUST_API_KEY` ve gerekliyse `BEYONDTRUST_RUNAS_USER` degerlerini kontrol edin.

## API URL Yanlis

- `BEYONDTRUST_API_URL`, BeyondTrust public API base URL degerine isaret etmelidir.
- Beklenen endpoint ailesi `Auth/Connect/Token`, `Auth/SignAppin`, `ManagedAccounts`, `Requests`, `Credentials/{id}`, `Requests/{id}/Checkin`, `Secrets-Safe/Secrets` endpoint'lerini icerir.

## SSL/TLS Error

- Production icin `BEYONDTRUST_IGNORE_SSL_ERRORS=false` kullanin.
- Private CA kullanan bir endpoint'e baglaniyorsaniz `BEYONDTRUST_CERTIFICATE_CONTENT` verin.
- `BEYONDTRUST_IGNORE_SSL_ERRORS=true` sadece demo veya kontrollu dev ortami icin uygundur.

## Secret Gelmiyor

- Key kullanimini birebir kontrol edin.
- Managed account key format'i `bt.acc.{SystemName}.{AccountName}` seklindedir.
- Secret Safe key format'lari `bt.safe.{Folder}.{Title}.password` ve `bt.safe.{Folder}.{Title}.username` seklindedir.

## Managed Account Bulunamadi

- `BEYONDTRUST_MANAGED_ACCOUNTS` degerinin BeyondTrust tarafindaki `SystemName.AccountName` ile birebir eslestigini kontrol edin.
- Tum erisilebilir account'lari yuklemek istiyorsaniz `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=true` kullanin.

## Secret Safe Path Yanlis

- `BEYONDTRUST_SECRET_SAFE_PATHS` degerini kontrol edin.
- Bu surumde `BEYONDTRUST_ALL_SECRETS_ENABLED` global enumerate davranisi baslatmaz.

## Refresh Calismiyor

- `BEYONDTRUST_REFRESH_INTERVAL=0` background refresh'i disabled yapar.
- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir ve iki refresh parameter birlikte verilirse her zaman kazanir.
- `BT_REFRESH_TIME`, canonical parameter yoksa kullanilan legacy alias'tir.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error olusur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa default value kullanilir.
- Refresh sirasinda bir hata olursa library son basarili snapshot'i korur.

## system property ve environment variable Sirasi

- Resolution sirasi `system property -> environment variable -> default value` seklindedir.
- Local demo app calistirirken `-D...` ile verilen `system property` degerleri shell seviyesindeki environment variable degerlerinden once okunur.

## Demo Example Output Gelmiyor

- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD` ve `BT_EXAMPLE_SAFE_USERNAME` demo-only helper parameter'lardir.
- Bu parameter'lari gercek bir `bt.*` key'ine isaret edecek sekilde set edin.
- Helper parameter set edilmemisse demo app ilgili example output icin skip mesaji yazar.
- Helper parameter var ama key yuklenmemisse demo app `Demo example key not found: <key>` mesaji yazar.
