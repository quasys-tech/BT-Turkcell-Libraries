# .NET Parameters

| Parameter | Required | Mode | Default Value | Aciklama | Example |
| --- | --- | --- | --- | --- | --- |
| `BEYONDTRUST_ENABLED` | No | Both | `true` | Provider'i enabled veya disabled yapar. Invalid boolean value validation error uretir. | `true` |
| `BEYONDTRUST_API_URL` | Yes | Both | - | BeyondTrust public API base URL degeridir. | `https://pam.example.com/BeyondTrust/api/public/v3` |
| `BEYONDTRUST_USE_APP_USER` | Yes when enabled | Both | - | `BEYONDTRUST_ENABLED=true` ise explicit verilmelidir. `true` degeri `OAuth`, `false` degeri `classic API auth` secer. | `false` |
| `BEYONDTRUST_CLIENT_ID` | Yes | OAuth | - | `OAuth` icin client ID degeridir. | `<CLIENT_ID>` |
| `BEYONDTRUST_CLIENT_SECRET` | Yes | OAuth | - | `OAuth` icin client secret degeridir. | `<CLIENT_SECRET>` |
| `BEYONDTRUST_API_KEY` | Yes | classic API auth | - | Raw API key veya `PS-Auth` format'i alir. | `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;` |
| `BEYONDTRUST_RUNAS_USER` | No | classic API auth | - | `runas` bilgisini ayri vermek icin kullanilir. | `<RUNAS_USER>` |
| `BEYONDTRUST_IGNORE_SSL_ERRORS` | No | Both | `false` | TLS validation'i sadece explicit olarak enabled yapildiginda kapatir. Invalid boolean value validation error uretir. | `false` |
| `BEYONDTRUST_CERTIFICATE_CONTENT` | No | Both | - | PEM certificate content degeridir. | `-----BEGIN CERTIFICATE-----...` |
| `BEYONDTRUST_REFRESH_INTERVAL` | No | Both | `1800` | Canonical refresh interval parameter'idir. Invalid value validation error uretir. `0` degeri background refresh'i disabled yapar. | `300` |
| `BT_REFRESH_TIME` | No | Both | - | Legacy alias'tir. Sadece `BEYONDTRUST_REFRESH_INTERVAL` yoksa ve parse edilebiliyorsa kullanilir. Invalid legacy value varsa default value kullanilir. | `300` |
| `BEYONDTRUST_MANAGED_ACCOUNTS` | No | Both | - | `;` ile ayrilan managed account hedef listesidir. | `LinuxProd.root;WindowsProd.administrator` |
| `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` | No | Both | `false` | API tarafindan donen tum managed account'lari yukler. Invalid boolean value validation error uretir. | `false` |
| `BEYONDTRUST_SECRET_SAFE_PATHS` | No | Both | - | `,` veya `;` ile ayrilan Secret Safe path listesidir. | `Team/Db,Team/Api` |
| `BEYONDTRUST_ALL_SECRETS_ENABLED` | No | Both | `false` | Compatibility flag'dir. Secret Safe yuklemesi yine path-based calisir. Invalid boolean value validation error uretir. | `false` |

## Shared Behavior Notes

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir.
- `BT_REFRESH_TIME` legacy alias'tir.
- Canonical parameter invalid ise validation error olusur.
- Legacy alias invalid ise ve canonical parameter yoksa default value kullanilir.
- Shared boolean parameter'lar `.NET` ve `Java` tarafinda ayni success/error/default davranisini izler.

## Demo-Only Helper Parameters

Bu parameter'lar library core configuration parameter'i degildir. Sadece demo app tarafinda example output secmek icin kullanilir.

| Parameter | Required | Mode | Default Value | Aciklama | Example |
| --- | --- | --- | --- | --- | --- |
| `BT_EXAMPLE_ACCOUNT` | No | Demo only | Not set | Demo app'in raw loglayacagi managed account key'ini secer. | `bt.acc.SampleSystem.SampleAccount` |
| `BT_EXAMPLE_SAFE_PASSWORD` | No | Demo only | Not set | Demo app'in raw loglayacagi Secret Safe password key'ini secer. | `bt.safe.SampleFolder.SampleTitle.password` |
| `BT_EXAMPLE_SAFE_USERNAME` | No | Demo only | Not set | Demo app'in raw loglayacagi Secret Safe username key'ini secer. | `bt.safe.SampleFolder.SampleTitle.username` |
