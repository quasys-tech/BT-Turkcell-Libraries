# Python Parameters

| Parameter | Required | Mode | Default Value | Açıklama | Example |
| --- | --- | --- | --- | --- | --- |
| `BEYONDTRUST_ENABLED` | No | Both | `true` | Manager'ı enabled veya disabled yapar. Invalid boolean value validation error üretir. | `true` |
| `BEYONDTRUST_API_URL` | Yes | Both | - | BeyondTrust public API base URL değeridir. | `https://pam.example.com/BeyondTrust/api/public/v3` |
| `BEYONDTRUST_USE_APP_USER` | Yes when enabled | Both | - | `BEYONDTRUST_ENABLED=true` ise explicit verilmelidir. `true` değeri `OAuth`, `false` değeri `classic API auth` seçer. | `false` |
| `BEYONDTRUST_CLIENT_ID` | Yes | OAuth | - | `OAuth` için client ID değeridir. | `<CLIENT_ID>` |
| `BEYONDTRUST_CLIENT_SECRET` | Yes | OAuth | - | `OAuth` için client secret değeridir. | `<CLIENT_SECRET>` |
| `BEYONDTRUST_API_KEY` | Yes | classic API auth | - | Raw API key veya `PS-Auth` format'ı alır. | `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;` |
| `BEYONDTRUST_RUNAS_USER` | No | classic API auth | - | `runas` bilgisini ayrı vermek için kullanılır. | `<RUNAS_USER>` |
| `BEYONDTRUST_IGNORE_SSL_ERRORS` | No | Both | `false` | TLS validation'ı sadece explicit olarak enabled yapıldığında kapatır. Invalid boolean value validation error üretir. | `false` |
| `BEYONDTRUST_CERTIFICATE_CONTENT` | No | Both | - | PEM certificate content değeridir. Runtime TLS verify için gerçek olarak kullanılır. | `-----BEGIN CERTIFICATE-----...` |
| `BEYONDTRUST_REFRESH_INTERVAL` | No | Both | `1800` | Canonical refresh interval parameter'ıdır. Invalid value validation error üretir. `0` değeri background refresh'i disabled yapar. | `300` |
| `BT_REFRESH_TIME` | No | Both | - | Legacy alias'tır. Sadece `BEYONDTRUST_REFRESH_INTERVAL` yoksa ve parse edilebiliyorsa kullanılır. Invalid legacy value varsa `default value` kullanılır. | `300` |
| `BEYONDTRUST_MANAGED_ACCOUNTS` | No | Both | - | `;` ile ayrılan managed account hedef listesidir. | `LinuxProd.root;WindowsProd.administrator` |
| `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` | No | Both | `false` | API tarafından dönen tüm managed account'ları yükler. Invalid boolean value validation error üretir. | `false` |
| `BEYONDTRUST_SECRET_SAFE_PATHS` | No | Both | - | `,` veya `;` ile ayrılan Secret Safe path listesidir. | `Team/Db,Team/Api` |
| `BEYONDTRUST_ALL_SECRETS_ENABLED` | No | Both | `false` | Compatibility flag'dir. Secret Safe yüklemesi yine path-based çalışır. Invalid boolean value validation error üretir. | `false` |

## Shared Behavior Notes

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` legacy alias'tır.
- Canonical parameter invalid ise validation error oluşur.
- Legacy alias invalid ise ve canonical parameter yoksa `default value` kullanılır.
- Shared boolean parameter'lar `Python`, `.NET` ve `Java` tarafında aynı success/error/default davranışını izler.

## Demo-Only Helper Parameters

Bu parameter'lar library core configuration parameter'ı değildir. Sadece demo app tarafında example output seçmek için kullanılır.

| Parameter | Required | Mode | Default Value | Açıklama | Example |
| --- | --- | --- | --- | --- | --- |
| `BT_EXAMPLE_ACCOUNT` | No | Demo only | Not set | Demo app'in raw loglayacağı managed account key'ini seçer. | `bt.acc.SampleSystem.SampleAccount` |
| `BT_EXAMPLE_SAFE_PASSWORD` | No | Demo only | Not set | Demo app'in raw loglayacağı Secret Safe password key'ini seçer. | `bt.safe.SampleFolder.SampleTitle.password` |
| `BT_EXAMPLE_SAFE_USERNAME` | No | Demo only | Not set | Demo app'in raw loglayacağı Secret Safe username key'ini seçer. | `bt.safe.SampleFolder.SampleTitle.username` |
