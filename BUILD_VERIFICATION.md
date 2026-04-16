# Build Verification

## Execution Time

- Verification completed at `2026-04-16T22:07:19.0739767+03:00`

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
| `dotnet build dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Passed | Release build completed. |
| `dotnet test dotnet/TurkcellBTDotnetSolution.sln -c Release` | `0` | Passed | `32/32` tests passed. |
| `mvn -f java/turkcell-bt-java-lib/pom.xml test` | `0` | Passed | `28/28` tests passed. |
| `mvn -f java/turkcell-bt-java-lib/pom-demo.xml package` | `0` | Passed | Demo shaded jar packaged successfully. |

Raw logs:

- [verification/dotnet-restore.txt](verification/dotnet-restore.txt)
- [verification/dotnet-build.txt](verification/dotnet-build.txt)
- [verification/dotnet-test.txt](verification/dotnet-test.txt)
- [verification/maven-test.txt](verification/maven-test.txt)
- [verification/maven-package.txt](verification/maven-package.txt)

## Parity Verified In This Round

- `BEYONDTRUST_REFRESH_INTERVAL` and `BT_REFRESH_TIME` now follow the same precedence, default, and error semantics in `.NET` and `Java`.
- Invalid canonical refresh values now fail in both libraries instead of silently falling back.
- Invalid legacy refresh values now fall back to the default only when the canonical setting is absent in both libraries.
- `BEYONDTRUST_USE_APP_USER` validation remains explicit in both libraries when `BEYONDTRUST_ENABLED=true`.
- Shared boolean parameters now fail fast on invalid values in both libraries:
  `BEYONDTRUST_ENABLED`, `BEYONDTRUST_USE_APP_USER`, `BEYONDTRUST_IGNORE_SSL_ERRORS`, `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED`, `BEYONDTRUST_ALL_SECRETS_ENABLED`.

## Failure Summary

- No command failures remained in the final verification run.

## Final Status

- `.NET build/test passed`
- `Java test/package passed`
- Shared parameter parity checks passed for refresh precedence, auth-mode validation, and shared boolean parsing.
