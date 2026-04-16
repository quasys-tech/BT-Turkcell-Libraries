# Turkcell.BT.Dotnet.Lib Package Notes

Bu package, BeyondTrust value'larını standart `.NET` configuration pipeline içine ekler.

## Behavior Özeti

- `builder.Configuration.AddBeyondTrustSecrets();` ile eklenir
- `OAuth` ve `classic API auth` destekler
- Canonical key format'larını değiştirmez
- Refresh başarısız olursa son başarılı snapshot'ı korur
- Fake `ERROR_*` secret value publish etmez

## Canonical Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Diğer Docs

- [../../README.md](../../README.md)
- [../../USAGE.md](../../USAGE.md)
- [../../PARAMETERS.md](../../PARAMETERS.md)
- [../../TROUBLESHOOTING.md](../../TROUBLESHOOTING.md)
