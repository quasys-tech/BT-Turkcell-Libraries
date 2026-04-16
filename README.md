# BT-Turkcell-Libraries

Bu repo, BeyondTrust entegrasyonu icin hazirlanan `.NET` ve `Java` library orneklerini ve ilgili docs setini icerir.

## Repo Icerigi

- `.NET` library ve ana docs: [dotnet/README.md](dotnet/README.md)
- `Java` library ve ana docs: [java/turkcell-bt-java-lib/README.md](java/turkcell-bt-java-lib/README.md)
- Build/test/package verification ozeti: [BUILD_VERIFICATION.md](BUILD_VERIFICATION.md)
- Raw verification output'lari: `verification/`

## Key Format Contract

Her iki library de ayni key formatlarini uretir:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Desteklenen Auth Mode'lar

- `OAuth / App User / Client Credentials`
- `classic API auth`

## Docs Haritasi

- `.NET` kullanim adimlari icin [dotnet/USAGE.md](dotnet/USAGE.md)
- `.NET` parametre ozeti icin [dotnet/PARAMETERS.md](dotnet/PARAMETERS.md)
- `.NET` troubleshooting notlari icin [dotnet/TROUBLESHOOTING.md](dotnet/TROUBLESHOOTING.md)
- `Java` kullanim adimlari icin [java/USAGE.md](java/USAGE.md)
- `Java` parametre ozeti icin [java/turkcell-bt-java-lib/PARAMETERS.md](java/turkcell-bt-java-lib/PARAMETERS.md)
- `Java` troubleshooting notlari icin [java/turkcell-bt-java-lib/TROUBLESHOOTING.md](java/turkcell-bt-java-lib/TROUBLESHOOTING.md)

## Verification Nasil Okunur

- Ozet durum icin once [BUILD_VERIFICATION.md](BUILD_VERIFICATION.md) dosyasina bakin.
- Her komutun raw output'unu gormek icin `verification/*.txt` file'larini acin.
- `BUILD_VERIFICATION.md` icindeki tum log referanslari `verification/` altindaki gercek file'larla birebir eslesir.
