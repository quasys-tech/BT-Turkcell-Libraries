# Java Usage

## Add The Library

Maven dependency example:

```xml
<dependency>
  <groupId>com.turkcell.bt.java</groupId>
  <artifactId>turkcell-bt-java-lib</artifactId>
  <version>1.0.0</version>
</dependency>
```

## Minimal Code

```java
try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String managedPassword = manager.getProperty("bt.acc.LinuxProd.root");
    String safePassword = manager.getProperty("bt.safe.Team/Db.AppDb.password");
}
```

## Local Windows

Classic API sample:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-apikey.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

OAuth sample:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-oauth.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

## Local Linux

Classic API sample:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-apikey.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

OAuth sample:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-oauth.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

## Kubernetes

Recommended manifests:

- Classic API: [turkcell-bt-java-lib/k8s/apikey-configmap.yml](turkcell-bt-java-lib/k8s/apikey-configmap.yml), [turkcell-bt-java-lib/k8s/apikey-secret.yml](turkcell-bt-java-lib/k8s/apikey-secret.yml), [turkcell-bt-java-lib/k8s/apikey-deployment.yml](turkcell-bt-java-lib/k8s/apikey-deployment.yml)
- OAuth: [turkcell-bt-java-lib/k8s/oauth-configmap.yml](turkcell-bt-java-lib/k8s/oauth-configmap.yml), [turkcell-bt-java-lib/k8s/oauth-secret.yml](turkcell-bt-java-lib/k8s/oauth-secret.yml), [turkcell-bt-java-lib/k8s/oauth-deployment.yml](turkcell-bt-java-lib/k8s/oauth-deployment.yml)

## Demo Application

The main demo app:

- reads Java system properties first, then environment variables
- supports both auth modes
- requires `BEYONDTRUST_USE_APP_USER` to be explicitly set in every enabled sample
- prints all loaded `bt.*` keys
- raw-logs the configured example managed account, Secret Safe password, and Secret Safe username keys
- accepts demo-only helper parameters to choose sample keys:
  `BT_EXAMPLE_ACCOUNT`, `BT_EXAMPLE_SAFE_PASSWORD`, `BT_EXAMPLE_SAFE_USERNAME`

Run:

```bash
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
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
- Local Java runs should still set `BEYONDTRUST_USE_APP_USER` explicitly through either a system property or an environment variable.
