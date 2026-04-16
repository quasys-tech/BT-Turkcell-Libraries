# Build Verification

## Overview

- Bu file, `BT-Turkcell-Libraries` repo'su için `2026-04-16T23:09:08.0946664+03:00` tarihinde tamamlanan final verification run'inin özetini verir.
- Bu turda code behavior değiştirilmedi. Sadece documentation dili profesyonel teslim seviyesine getirildi ve `verification/` altındaki log file'ları yeniden üretildi.
- Verification log'ları gerçek command output'larından üretildi, ardından teslim paketi için sanitize edildi.

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

| Command | Exit Code | Result | Kısa sonuç | Raw log |
| --- | --- | --- | --- | --- |
| `dotnet --info` | `0` | Pass | Toolchain bilgisi alındı. | [verification/dotnet-info.txt](verification/dotnet-info.txt) |
| `mvn -v` | `0` | Pass | Maven ve Java version bilgisi alındı. | [verification/maven-version.txt](verification/maven-version.txt) |
| `dotnet restore dotnet/TurkcellBTDotnetSolution.sln` | `0` | Pass | Solution restore tamamlandı. | [verification/dotnet-restore.txt](verification/dotnet-restore.txt) |
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | Release build `0 Warning`, `0 Error` ile tamamlandı. | [verification/dotnet-build.txt](verification/dotnet-build.txt) |
| `dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | `.NET` test run `32/32` passed. | [verification/dotnet-test.txt](verification/dotnet-test.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom.xml test` | `0` | Pass | `Java` test run `28/28` passed ve `BUILD SUCCESS` üretildi. | [verification/maven-test.txt](verification/maven-test.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom-demo.xml package` | `0` | Pass | Demo `package` run `BUILD SUCCESS` ile tamamlandı. | [verification/maven-package.txt](verification/maven-package.txt) |

## Verification Scope

- `verification/` klasörü final teslim için yeniden üretildi.
- Tüm verification log'ları UTF-8 plain text olarak yazıldı.
- `.NET` tarafında `restore`, `build` ve `test` komutları tekrar koşturuldu.
- `Java` tarafında `test` ve demo `package` komutları tekrar koşturuldu.
- Root `README.md`, `.NET` docs seti ve `Java` docs seti Türkçe anlatım + English teknik terim standardına getirildi.
- `BUILD_VERIFICATION.md` içindeki tüm log referansları mevcut `verification/*.txt` file'larıyla birebir eşleştirildi.

## Notes

- Verification log'ları gerçek command output'larından üretilmiştir.
- Teslim paketi için local path, local username ve machine-specific absolute path izleri sanitize edilmiştir.
- Sanitization sırasında şu placeholder'lar kullanılmıştır:
  - `<local-user>`
  - `<workspace>`
  - `<local-path>`
- Bu sanitization, command result, exit code, tool version, error meaning veya build/test/package sonucunu değiştirmez.
- `verification/maven-test.txt` içinde görülen `simulated refresh failure` satırı test coverage'in bilinçli bir parçasıdır. Command exit code `0` olduğu ve Maven sonucu `BUILD SUCCESS` olduğu için verification sonucu değişmez.
- `verification/maven-test.txt` ve `verification/maven-package.txt` içindeki warning satırları final sonucu fail etmemiştir.
- Final durum:
  - `.NET build/test passed`
  - `Java test/package passed`
