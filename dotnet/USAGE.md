# .NET Usage

## Add The Library

Package reference example:

```bash
dotnet add package Turkcell.BT.Dotnet.Lib
```

Project reference example inside this repo:

```xml
<ProjectReference Include="..\src\Turkcell.BT.Dotnet.Lib\Turkcell.BT.Dotnet.Lib.csproj" />
```

## Minimal Code

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

using var host = builder.Build();
var configuration = host.Services.GetRequiredService<IConfiguration>();

var managedPassword = configuration["bt.acc.LinuxProd.root"];
var safePassword = configuration["bt.safe.Team/Db.AppDb.password"];
```

## Local Windows

Classic API sample:

```powershell
. .\examples\env\windows-apikey.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

OAuth sample:

```powershell
. .\examples\env\windows-oauth.ps1.sample
dotnet run --project .\samples\Turkcell.BT.Dotnet.Demo
```

## Local Linux

Classic API sample:

```bash
source ./examples/env/linux-apikey.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

OAuth sample:

```bash
source ./examples/env/linux-oauth.sh.sample
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

## Kubernetes

Recommended manifests:

- Classic API: [k8s/apikey-configmap.yml](k8s/apikey-configmap.yml), [k8s/apikey-secret.yml](k8s/apikey-secret.yml), [k8s/apikey-deployment.yml](k8s/apikey-deployment.yml)
- OAuth: [k8s/oauth-configmap.yml](k8s/oauth-configmap.yml), [k8s/oauth-secret.yml](k8s/oauth-secret.yml), [k8s/oauth-deployment.yml](k8s/oauth-deployment.yml)

## Demo Application

The demo app:

- reads env variables only
- supports both auth modes
- requires `BEYONDTRUST_USE_APP_USER` to be explicitly set in every enabled sample
- prints all loaded `bt.*` keys
- raw-logs the configured example managed account, Secret Safe password, and Secret Safe username keys
- accepts demo-only helper parameters to choose sample keys:
  `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD`, `BT_EXAMPLE_SAFE_USERNAME`

Run:

```bash
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

Demo helper examples:

- `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`
- If one of these parameters is missing, the demo prints a skip message for that specific output.
- If one of these parameters points to a key that is not loaded, the demo prints `Demo example key not found: <key>`.

## OAuth Scenario

Required variables:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true`
- `BEYONDTRUST_CLIENT_ID=<CLIENT_ID>`
- `BEYONDTRUST_CLIENT_SECRET=<CLIENT_SECRET>`

## Classic API Scenario

Required variables:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=false`
- `BEYONDTRUST_API_KEY=<API_KEY>` or `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;`
- `BEYONDTRUST_RUNAS_USER=<RUNAS_USER>` when you want to supply `runas` separately

## Refresh Interval Note

- Use `BEYONDTRUST_REFRESH_INTERVAL` as the canonical setting.
- `BT_REFRESH_TIME` is accepted only for backward compatibility when the canonical setting is absent.
- If `BEYONDTRUST_REFRESH_INTERVAL` is present but invalid, treat it as a configuration error instead of falling back to `BT_REFRESH_TIME`.
