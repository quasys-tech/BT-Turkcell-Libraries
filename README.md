# BT-Turkcell-Libraries

Bu repo, BeyondTrust entegrasyonu için hazırlanan `.NET`, `Java` ve `Python` library örneklerini, demo app'leri ve ilgili docs setini içerir.

## Repo İçeriği

- `.NET` library ve ana docs: [dotnet/README.md](dotnet/README.md)
- `Java` library ve ana docs: [java/turkcell-bt-java-lib/README.md](java/turkcell-bt-java-lib/README.md)
- `Python` library ve ana docs: [python/README.md](python/README.md)
- Build/test/package verification özeti: [BUILD_VERIFICATION.md](BUILD_VERIFICATION.md)
- Raw verification output'ları: `verification/`

## Key Format Contract

Her üç library de aynı key formatlarını üretir:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Docs Haritası

- `.NET` kullanım adımları için [dotnet/USAGE.md](dotnet/USAGE.md)
- `.NET` parameter özeti için [dotnet/PARAMETERS.md](dotnet/PARAMETERS.md)
- `.NET` troubleshooting notları için [dotnet/TROUBLESHOOTING.md](dotnet/TROUBLESHOOTING.md)
- `Java` kullanım adımları için [java/USAGE.md](java/USAGE.md)
- `Java` parameter özeti için [java/turkcell-bt-java-lib/PARAMETERS.md](java/turkcell-bt-java-lib/PARAMETERS.md)
- `Java` troubleshooting notları için [java/turkcell-bt-java-lib/TROUBLESHOOTING.md](java/turkcell-bt-java-lib/TROUBLESHOOTING.md)
- `Python` kullanım adımları için [python/USAGE.md](python/USAGE.md)
- `Python` parameter özeti için [python/PARAMETERS.md](python/PARAMETERS.md)
- `Python` troubleshooting notları için [python/TROUBLESHOOTING.md](python/TROUBLESHOOTING.md)

## Verification Nasıl Okunur

- Özet durum için önce [BUILD_VERIFICATION.md](BUILD_VERIFICATION.md) dosyasına bakın.
- Her komutun raw output'unu görmek için `verification/*.txt` file'larını açın.
- `BUILD_VERIFICATION.md` içindeki tüm log referansları `verification/` altındaki gerçek file'larla birebir eşleşir.
