# Java Usage

## Library Ekleme

Maven dependency örneği:

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

`classic API auth` örneği:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-apikey.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

`OAuth` örneği:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-oauth.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

## Local Linux

`classic API auth` örneği:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-apikey.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

`OAuth` örneği:

```bash
source ./turkcell-bt-java-lib/examples/env/linux-oauth.sh.sample
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

## Kubernetes

Önerilen manifest setleri:

- `classic API auth`: [turkcell-bt-java-lib/k8s/apikey-configmap.yml](turkcell-bt-java-lib/k8s/apikey-configmap.yml), [turkcell-bt-java-lib/k8s/apikey-secret.yml](turkcell-bt-java-lib/k8s/apikey-secret.yml), [turkcell-bt-java-lib/k8s/apikey-deployment.yml](turkcell-bt-java-lib/k8s/apikey-deployment.yml)
- `OAuth`: [turkcell-bt-java-lib/k8s/oauth-configmap.yml](turkcell-bt-java-lib/k8s/oauth-configmap.yml), [turkcell-bt-java-lib/k8s/oauth-secret.yml](turkcell-bt-java-lib/k8s/oauth-secret.yml), [turkcell-bt-java-lib/k8s/oauth-deployment.yml](turkcell-bt-java-lib/k8s/oauth-deployment.yml)

## Demo App

Ana demo app:

- önce `system property`, sonra `environment variable` okur
- iki auth mode'u da destekler
- `BEYONDTRUST_USE_APP_USER` değerinin explicit verilmesini bekler
- yüklenen tüm `bt.*` key'lerini yazdırır
- seçilen example managed account, Secret Safe password ve Secret Safe username key'lerini raw loglar

Çalıştırma komutu:

```bash
mvn -f ./turkcell-bt-java-lib/pom-demo.xml -DskipTests package
java -jar ./turkcell-bt-java-lib/target/turkcell-bt-java-demo-shaded.jar
```

Demo-only helper parameter örnekleri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.SampleSystem.SampleAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.SampleFolder.SampleTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.SampleFolder.SampleTitle.username`
- Bir helper parameter set edilmemişse demo app ilgili example output için skip mesajı yazar.
- Bir helper parameter yüklenmiş bir key'e işaret etmiyorsa demo app `Demo example key not found: <key>` mesajı yazar.

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
- `BEYONDTRUST_RUNAS_USER=<RUNAS_USER>` değerini `runas` bilgisini ayrı vermek istiyorsanız kullanın

## Refresh Interval Notu

- `BEYONDTRUST_REFRESH_INTERVAL` canonical parameter'dır.
- `BT_REFRESH_TIME` sadece backward compatibility için desteklenen legacy alias'tır.
- `BEYONDTRUST_REFRESH_INTERVAL` invalid ise validation error oluşur.
- `BT_REFRESH_TIME` invalid ise ve canonical parameter yoksa `default value` kullanılır.
