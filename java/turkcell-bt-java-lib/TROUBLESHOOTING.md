# Java Troubleshooting

## Auth Failed

- Check `BEYONDTRUST_USE_APP_USER`.
- For OAuth, verify `BEYONDTRUST_CLIENT_ID` and `BEYONDTRUST_CLIENT_SECRET`.
- For Classic API, verify `BEYONDTRUST_API_KEY` and optional `BEYONDTRUST_RUNAS_USER`.

## API URL Wrong

- `BEYONDTRUST_API_URL` must point to the BeyondTrust public API base.
- Expected endpoint family includes `Auth/Connect/Token`, `Auth/SignAppin`, `ManagedAccounts`, `Requests`, `Credentials/{id}`, `Requests/{id}/Checkin`, `Secrets-Safe/Secrets`.

## SSL/TLS Error

- Keep `BEYONDTRUST_IGNORE_SSL_ERRORS=false` in production.
- If your endpoint uses a private CA, provide `BEYONDTRUST_CERTIFICATE_CONTENT`.
- `BEYONDTRUST_IGNORE_SSL_ERRORS=true` is for demo or controlled dev use only.

## Secret Not Returned

- Check exact key usage.
- Managed accounts use `bt.acc.{SystemName}.{AccountName}`.
- Secret Safe values use `bt.safe.{Folder}.{Title}.password` and `bt.safe.{Folder}.{Title}.username`.

## Managed Account Not Found

- Verify `BEYONDTRUST_MANAGED_ACCOUNTS` exactly matches the `SystemName.AccountName` returned by BeyondTrust.
- If you want every accessible account, set `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=true`.

## Secret Safe Path Wrong

- Verify `BEYONDTRUST_SECRET_SAFE_PATHS`.
- `BEYONDTRUST_ALL_SECRETS_ENABLED` does not trigger global enumeration in this version.

## Refresh Not Working

- `BEYONDTRUST_REFRESH_INTERVAL=0` disables background refresh.
- `BT_REFRESH_TIME` is accepted, but `BEYONDTRUST_REFRESH_INTERVAL` is the canonical setting.
- On refresh failure the library keeps the last successful snapshot by design.

## Java System Property Vs Environment Variable

- Resolution order is `System property -> Environment variable -> Default value`.
- Local demo runs can use `-D...` system properties without changing shell env values.
