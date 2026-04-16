# .NET Parameters

| Parameter | Required | Mode | Default | Description | Example |
| --- | --- | --- | --- | --- | --- |
| `BEYONDTRUST_ENABLED` | No | Both | `true` | Enables or disables the provider. | `true` |
| `BEYONDTRUST_API_URL` | Yes | Both | - | BeyondTrust API base URL. | `https://pam.example.com/BeyondTrust/api/public/v3` |
| `BEYONDTRUST_USE_APP_USER` | Yes | Both | `false` | Selects OAuth (`true`) or Classic API (`false`). | `false` |
| `BEYONDTRUST_CLIENT_ID` | Yes | OAuth | - | OAuth client identifier. | `<CLIENT_ID>` |
| `BEYONDTRUST_CLIENT_SECRET` | Yes | OAuth | - | OAuth client secret. | `<CLIENT_SECRET>` |
| `BEYONDTRUST_API_KEY` | Yes | Classic API | - | Classic API key, raw or `PS-Auth` format. | `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;` |
| `BEYONDTRUST_RUNAS_USER` | No | Classic API | - | Optional `runas` value supplied separately. | `<RUNAS_USER>` |
| `BEYONDTRUST_IGNORE_SSL_ERRORS` | No | Both | `false` | Disables TLS validation only when explicitly enabled. | `false` |
| `BEYONDTRUST_CERTIFICATE_CONTENT` | No | Both | - | PEM certificate content used for custom TLS trust. | `-----BEGIN CERTIFICATE-----...` |
| `BEYONDTRUST_REFRESH_INTERVAL` | No | Both | `1800` | Refresh interval in seconds. Use `0` to disable background refresh. | `300` |
| `BEYONDTRUST_MANAGED_ACCOUNTS` | No | Both | - | Semicolon-separated managed account targets. | `LinuxProd.root;WindowsProd.administrator` |
| `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` | No | Both | `false` | Loads all managed accounts returned by the API. | `false` |
| `BEYONDTRUST_SECRET_SAFE_PATHS` | No | Both | - | Comma or semicolon separated Secret Safe paths. | `Team/Db,Team/Api` |
| `BEYONDTRUST_ALL_SECRETS_ENABLED` | No | Both | `false` | Compatibility flag. Secret Safe loading still stays path-based. | `false` |
