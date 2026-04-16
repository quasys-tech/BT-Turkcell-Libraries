# BT-Turkcell-Libraries

Production-ready BeyondTrust integration samples and libraries for:

- `.NET` in [dotnet/README.md](dotnet/README.md)
- `Java` in [java/turkcell-bt-java-lib/README.md](java/turkcell-bt-java-lib/README.md)

Both libraries keep the same key format contract:

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

Both libraries support:

- OAuth / App User / Client Credentials
- Classic API authentication
- Safe refresh semantics with last successful snapshot protection
- Optional custom certificate content via `BEYONDTRUST_CERTIFICATE_CONTENT`

Sample environment scripts and Kubernetes manifests are provided under:

- `.NET`: [dotnet/examples](dotnet/examples) and [dotnet/k8s](dotnet/k8s)
- `Java`: [java/turkcell-bt-java-lib/examples](java/turkcell-bt-java-lib/examples) and [java/turkcell-bt-java-lib/k8s](java/turkcell-bt-java-lib/k8s)
