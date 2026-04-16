# Turkcell.BT.Dotnet.Lib Package Notes

Bu package, BeyondTrust value'larini standart `.NET` configuration pipeline icine ekler.

## Behavior Ozeti

- `builder.Configuration.AddBeyondTrustSecrets();` ile eklenir
- `OAuth` ve `classic API auth` destekler
- Canonical key format'larini degistirmez
- Refresh basarisiz olursa son basarili snapshot'i korur
- Fake `ERROR_*` secret value publish etmez

## Canonical Key Formatlari

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Diger Docs

- [../../README.md](../../README.md)
- [../../USAGE.md](../../USAGE.md)
- [../../PARAMETERS.md](../../PARAMETERS.md)
- [../../TROUBLESHOOTING.md](../../TROUBLESHOOTING.md)
