# Build Verification

## Execution Time

- Verification completed at `2026-04-16T21:34:35.4359571+03:00`

## Toolchain

- `.NET SDK`: `8.0.420`
- `.NET Host`: `8.0.26`
- `Maven`: `3.9.12`
- `Java`: `21.0.10`

Raw logs:

- [verification/dotnet-info.txt](verification/dotnet-info.txt)
- [verification/maven-version.txt](verification/maven-version.txt)

## Commands

| Command | Exit Code | Result | Notes |
| --- | --- | --- | --- |
| `dotnet --info` | `0` | Passed | Toolchain available. |
| `mvn -v` | `0` | Passed | Toolchain available. |
| `dotnet restore dotnet/TurkcellBTDotnetSolution.sln` | `0` | Passed | Solution restore completed. |
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `1` | Failed on first attempt | `CS8956` in `dotnet/samples/Turkcell.BT.Dotnet.Demo/Program.cs`: file-scoped namespace was placed after top-level statements. |
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Passed on rerun | Re-ran after switching the demo helper namespace to block form. |
| `dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Passed | `25/25` tests passed. |
| `mvn -f java/turkcell-bt-java-lib/pom.xml test` | `0` | Passed | `18/18` tests passed. |
| `mvn -f java/turkcell-bt-java-lib/pom-demo.xml package` | `0` | Passed | Demo shaded jar packaged successfully. |

Raw logs:

- [verification/dotnet-restore.txt](verification/dotnet-restore.txt)
- [verification/dotnet-build.txt](verification/dotnet-build.txt)
- [verification/dotnet-test.txt](verification/dotnet-test.txt)
- [verification/maven-test.txt](verification/maven-test.txt)
- [verification/maven-package.txt](verification/maven-package.txt)

## Failure Summary

- The first `.NET build` attempt failed with `CS8956` because the new demo helper used a file-scoped namespace after top-level statements.
- After changing that helper to a block namespace in the same file, the required `.NET build` and `.NET test` commands passed.

## Final Status

- `.NET build/test passed`
- `Java test/package passed`
