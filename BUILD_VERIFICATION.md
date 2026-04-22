# Build Verification

## Overview

- Bu file, `BT-Turkcell-Libraries` repo'su için mevcut verification paketinin özetini verir.
- `.NET` ve `Java` log seti `2026-04-16T23:09:08.0946664+03:00` tarihli final verification run'inden korunmuştur.
- `Python` log seti `2026-04-16T23:58:59.0702198+03:00` tarihinde yeniden üretilmiştir.
- Verification log'ları gerçek command output'larından üretilmiştir. Teslim paketi için local path, local username ve machine-specific absolute path izleri sanitize edilmiştir.

## Toolchain

- `.NET SDK`: `8.0.420`
- `.NET Host`: `8.0.26`
- `Maven`: `3.9.12`
- `Java`: `21.0.10`
- `Python`: `3.13.1`
- `pip`: `24.3.1`

Raw logs:

- [verification/dotnet-info.txt](verification/dotnet-info.txt)
- [verification/maven-version.txt](verification/maven-version.txt)
- [verification/python-version.txt](verification/python-version.txt)

## Executed Commands

`.NET` ve `Java` verification seti:

```powershell
dotnet --info
mvn -v
dotnet restore dotnet/TurkcellBTDotnetSolution.sln
dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release
dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release
mvn -f java/turkcell-bt-java-lib/pom.xml test
mvn -f java/turkcell-bt-java-lib/pom-demo.xml package
```

`Python` verification seti:

```powershell
python --version
python -m pip --version
python -m pip install -e ./python[dev]
python -m pytest python/tests -q
python -m build ./python
```

## Results

| Command | Exit Code | Result | Kısa sonuç | Raw log |
| --- | --- | --- | --- | --- |
| `dotnet --info` | `0` | Pass | Toolchain bilgisi alındı. | [verification/dotnet-info.txt](verification/dotnet-info.txt) |
| `dotnet restore dotnet/TurkcellBTDotnetSolution.sln` | `0` | Pass | Solution restore tamamlandı. | [verification/dotnet-restore.txt](verification/dotnet-restore.txt) |
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | Release build `0 Warning`, `0 Error` ile tamamlandı. | [verification/dotnet-build.txt](verification/dotnet-build.txt) |
| `dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Pass | `.NET` test run `32/32` passed. | [verification/dotnet-test.txt](verification/dotnet-test.txt) |
| `mvn -v` | `0` | Pass | Maven ve Java version bilgisi alındı. | [verification/maven-version.txt](verification/maven-version.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom.xml test` | `0` | Pass | `Java` test run `28/28` passed ve `BUILD SUCCESS` üretildi. | [verification/maven-test.txt](verification/maven-test.txt) |
| `mvn -f java/turkcell-bt-java-lib/pom-demo.xml package` | `0` | Pass | Demo `package` run `BUILD SUCCESS` ile tamamlandı. | [verification/maven-package.txt](verification/maven-package.txt) |
| `python --version` ve `python -m pip --version` | `0` | Pass | `Python 3.13.1` ve `pip 24.3.1` doğrulandı. | [verification/python-version.txt](verification/python-version.txt) |
| `python -m pip install -e ./python[dev]` | `0` | Pass | Editable install ve `dev` dependency seti tamamlandı. | [verification/python-install.txt](verification/python-install.txt) |
| `python -m pytest python/tests -q` | `0` | Pass | `45/45` test passed. | [verification/python-pytest.txt](verification/python-pytest.txt) |
| `python -m build ./python` | `0` | Pass | `sdist` ve `wheel` build tamamlandı. | [verification/python-package.txt](verification/python-package.txt) |

## Verification Scope

- `verification/` altındaki `.NET`, `Java` ve `Python` log file'ları mevcut teslim paketinin doğrulama kaynağıdır.
- `Python` tarafında install, test ve package zinciri gerçek komutlarla çalıştırılmıştır.
- `Python` library için package, tests, demo app, docs, env samples ve Kubernetes örnekleri repo'ya eklenmiştir.
- Root `README.md` ve navigation yapısı üç implementation'ı gösterecek şekilde güncellenmiştir.

## Notes

- Sanitization sırasında şu placeholder'lar kullanılmıştır:
  - `<local-user>`
  - `<workspace>`
  - `<local-path>`
- Bu sanitization, command result, exit code, tool version, error meaning veya build/test/package sonucunu değiştirmez.
- `verification/maven-test.txt` içinde görülen `simulated refresh failure` satırı test coverage'in bilinçli bir parçasıdır. Command exit code `0` olduğu ve Maven sonucu `BUILD SUCCESS` olduğu için verification sonucu değişmez.
- `verification/maven-test.txt`, `verification/maven-package.txt`, `verification/python-install.txt` ve `verification/python-package.txt` içindeki warning satırları final sonucu fail etmemiştir.
- Final durum:
  - `.NET build/test passed`
  - `Java test/package passed`
  - `Python install/test/package passed`
