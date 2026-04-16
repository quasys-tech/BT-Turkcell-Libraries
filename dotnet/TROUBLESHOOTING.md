# .NET Troubleshooting

## Auth Failed

- Check `BEYONDTRUST_USE_APP_USER`.
- When `BEYONDTRUST_ENABLED=true`, `BEYONDTRUST_USE_APP_USER` must be explicitly set to `true` or `false`.
- Invalid boolean values for `BEYONDTRUST_ENABLED`, `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_IGNORE_SSL_ERRORS`, `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED`, or `BEYONDTRUST_ALL_SECRETS_ENABLED` are configuration errors.
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
- `BEYONDTRUST_REFRESH_INTERVAL` is the canonical setting and wins when both refresh variables are present.
- `BT_REFRESH_TIME` is legacy-only and is used only when the canonical setting is absent and valid.
- If `BEYONDTRUST_REFRESH_INTERVAL` is present but invalid, fix that value instead of expecting a silent fallback to `BT_REFRESH_TIME`.
- If `BT_REFRESH_TIME` is invalid and `BEYONDTRUST_REFRESH_INTERVAL` is absent, the default refresh interval is used.
- On refresh failure the library keeps the last successful snapshot by design.

## Demo Sample Key Not Printed

- `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD`, and `BT_EXAMPLE_SAFE_USERNAME` are demo-only helper parameters.
- Set them to existing `bt.*` keys, for example `bt.acc.SampleSystem.SampleAccount` or `bt.safe.SampleFolder.SampleTitle.password`.
- If a helper parameter is absent, the demo prints a skip message for that specific sample output.
- If a helper parameter points to a key that was not loaded, the demo prints `Demo example key not found: <key>`.

## Configuration Load Expectation

- `builder.Configuration.AddBeyondTrustSecrets();` adds a configuration provider.
- Values appear under normal `IConfiguration` keys, not through a separate API surface.
