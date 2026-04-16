# Turkcell.BT.Dotnet.Lib Package Notes

This package adds BeyondTrust values into the normal `.NET` configuration pipeline.

Behavior summary:

- Uses `builder.Configuration.AddBeyondTrustSecrets();`
- Supports OAuth and Classic API auth
- Keeps the canonical key formats unchanged
- Preserves the last successful snapshot when a refresh fails
- Does not publish fake `ERROR_*` secret values

Canonical key formats:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

See the higher-level consumer guides in:

- [../../README.md](../../README.md)
- [../../USAGE.md](../../USAGE.md)
- [../../PARAMETERS.md](../../PARAMETERS.md)
- [../../TROUBLESHOOTING.md](../../TROUBLESHOOTING.md)
