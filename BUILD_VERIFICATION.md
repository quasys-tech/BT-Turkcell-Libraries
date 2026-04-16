# Build Verification

## Overview

- Bu file, `BT-Turkcell-Libraries` repo'su icin `2026-04-16T22:27:19.4509046+03:00` tarihinde tekrar uretilen verification run'inin ozetini verir.
- Bu turda code behavior degistirilmedi. Sadece documentation dili standardize edildi ve `verification/` altindaki raw output file'lari yeniden olusturuldu.
- Bu ozet icindeki tum log referanslari repo root altindaki gercek `verification/*.txt` file'lariyla birebir uyumludur.

## Toolchain

- `.NET SDK`: `8.0.420`
- `.NET Host`: `8.0.26`
- `Maven`: `3.9.12`
- `Java`: `21.0.10`

Raw logs:

- [verification/dotnet-info.txt](verification/dotnet-info.txt)
- [verification/maven-version.txt](verification/maven-version.txt)

## Executed Commands

```powershell
dotnet --info
mvn -v
dotnet restore dotnet/TurkcellBTDotnetSolution.sln
dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release
dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release
mvn -f java/turkcell-bt-java-lib/pom.xml test
mvn -f java/turkcell-bt-java-lib/pom-demo.xml package
```

## Results

| Command | Exit Code | Result | Kisa sonuc | Raw log |
| --- | --- | --- | --- | --- |
| `dotnet --info` | `0` | Pass | Toolchain bilgisi alindi. | [verification/dotnet-info.txt](verification/dotnet-info.txt) |
| `mvn -v` | `0` | Pass | Maven ve Java version bilgisi alindi. | [verification/maven-version.txt](verification/maven-version.txt) |
| `dotnet restore dotnet/TurkcellBTDotnetSolution.sln` | `0` | Pass | Solution restore tamamlandi. | [verification/dotnet-restore.txt](verification/dotnet-restore.txt) |
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | Release build `0 Warning`, `0 Error` ile tamamlandi. | [verification/dotnet-build.txt](verification/dotnet-build.txt) |
| `dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | `.NET` test run `32/32` passed. | [verification/dotnet-test.txt](verification/dotnet-test.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom.xml test` | `0` | Pass | `Java` test run `28/28` passed ve `BUILD SUCCESS` uretildi. | [verification/maven-test.txt](verification/maven-test.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom-demo.xml package` | `0` | Pass | Demo package build `BUILD SUCCESS` ile tamamlandi. | [verification/maven-package.txt](verification/maven-package.txt) |

## Verification Scope

- `verification/` klasoru bu turda yeniden olusturuldu.
- `.NET` tarafinda `restore`, `build` ve `test` komutlari tekrar kosuldu.
- `Java` tarafinda `test` ve demo `package` komutlari tekrar kosuldu.
- `BUILD_VERIFICATION.md` icindeki log referanslari mevcut file isimleriyle birebir eslestirildi.
- Root `README.md`, `.NET` ve `Java` docs seti Turkce anlatim + English teknik terim standardina getirildi.

## Notes

- `verification/maven-test.txt` icinde gorulen `simulated refresh failure` satiri test coverage'in bilincli bir parcasidir. Command exit code `0` oldugu ve Maven sonucu `BUILD SUCCESS` oldugu icin verification sonucu degismez.
- `verification/maven-test.txt` ve `verification/maven-package.txt` icinde warning satirlari vardir. Bu warning'ler package/test sonucunu fail etmemistir.
- Final durum:
  - `.NET build/test passed`
  - `Java test/package passed`
