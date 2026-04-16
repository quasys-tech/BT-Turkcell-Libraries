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
- prints all loaded `bt.*` keys
- prints one managed account value and one Secret Safe password value as raw output

Run:

```bash
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
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
