# Java Usage

## Library Ekleme

Maven dependency ornegi:

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

`classic API auth` ornegi:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-apikey.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

`OAuth` ornegi:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-oauth.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

## Local Linux

`classic API auth` ornegi:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-apikey.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

`OAuth` ornegi:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-oauth.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

## Kubernetes

Onerilen manifest setleri:

- `classic API auth`: [turkcell-bt-java-lib/k8s/apikey-configmap.yml](turkcell-bt-java-lib/k8s/apikey-configmap.yml), [turkcell-bt-java-lib/k8s/apikey-secret.yml](turkcell-bt-java-lib/k8s/apikey-secret.yml), [turkcell-bt-java-lib/k8s/apikey-deployment.yml](turkcell-bt-java-lib/k8s/apikey-deployment.yml)
- `OAuth`: [turkcell-bt-java-lib/k8s/oauth-configmap.yml](turkcell-bt-java-lib/k8s/oauth-configmap.yml), [turkcell-bt-java-lib/k8s/oauth-secret.yml](turkcell-bt-java-lib/k8s/oauth-secret.yml), [turkcell-bt-java-lib/k8s/oauth-deployment.yml](turkcell-bt-java-lib/k8s/oauth-deployment.yml)

## Demo App

Ana demo app:

- once `system property`, sonra `environment variable` okur
- iki auth mode'u da destekler
- `BEYONDTRUST_USE_APP_USER` degerinin explicit verilmesini bekler
- yuklenen tum `bt.*` key'lerini yazdirir
- secilen example managed account, Secret Safe password ve Secret Safe username key'lerini raw loglar

Calistirma komutu:

```bash
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

Demo-only helper parameter ornekleri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`
- Bir helper parameter set edilmemisse demo app ilgili example output icin skip mesaji yazar.
- Bir helper parameter yuklenmis bir key'e isaret etmiyorsa demo app `Demo example key not found: <key>` mesaji yazar.

## OAuth Senaryosu

Gerekli parameter'lar:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true`
- `BEYONDTRUST_CLIENT_ID=<CLIENT_ID>`
- `BEYONDTRUST_CLIENT_SECRET=<CLIENT_SECRET>`

## classic API auth Senaryosu

Gerekli parameter'lar:

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=false`
- `BEYONDTRUST_API_KEY=<API_KEY>` veya `PS-Auth key=<API_KEY>; runas=<RUNAS_USER>;`
- `BEYONDTRUST_RUNAS_USER=<RUNAS_USER>` degerini `runas` bilgisini ayri vermek istiyorsaniz kullanin

## Refresh Interval Notu

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dir.
- `BT_REFRESH_TIME` sadece backward compatibility icin desteklenen legacy alias'tir.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error olusur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa default value kullanilir.
