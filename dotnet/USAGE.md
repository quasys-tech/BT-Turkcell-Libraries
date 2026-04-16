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
- prints all loaded `bt.*` keys
- prints one managed account value and one Secret Safe password value as raw output

Run:

```bash
dotnet run --project ./samples/Turkcell.BT.Dotnet.Demo
```

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
