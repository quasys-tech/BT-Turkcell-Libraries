# Java Usage

## Önerilen Entegrasyon Akışı

1. Library'yi Artifactory üzerinden Maven dependency olarak ekleyin.
2. Uygulama başlangıcında BeyondTrust environment variable'larını verin.
3. `BeyondTrustConfigurationManager.createAndLoad()` ile manager'ı oluşturun.
4. Uygulama içinde canonical `bt.*` key'leri üzerinden değerleri okuyun.

## Artifactory Örneği

```xml
<repositories>
  <repository>
    <id>bt-artifactory</id>
    <url>https://<ARTIFACTORY_HOST>/artifactory/<MAVEN_REPO_KEY></url>
  </repository>
</repositories>

<dependency>
  <groupId>com.turkcell.bt.java</groupId>
  <artifactId>turkcell-bt-java-lib</artifactId>
  <version><VERSION></version>
</dependency>
```

## Minimal Kod

```java
try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String managedPassword = manager.getProperty("bt.acc.MySystem.MyAccount");
    String safePassword = manager.getProperty("bt.safe.MyFolder.MyTitle.password");
    String safeUsername = manager.getProperty("bt.safe.MyFolder.MyTitle.username");
}
```

## Zorunlu Ayarlar

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true` veya `false`
- `OAuth` için: `BEYONDTRUST_CLIENT_ID`, `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için: `BEYONDTRUST_API_KEY`, opsiyonel `BEYONDTRUST_RUNAS_USER`
- Yüklenecek hedefler için: `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`

## POC ile Hızlı Doğrulama

`POC`, sadece seçilen 3 örnek key'i yazar, refresh açıksa süreç açık kalır ve çıktı değiştiğinde blok halinde tekrar basar.

Classic API:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-apikey.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

OAuth:

```powershell
. .\turkcell-bt-java-lib\examples\env\windows-oauth.ps1.sample
mvn -f .\turkcell-bt-java-lib\pom-demo.xml -DskipTests package
java -jar .\turkcell-bt-java-lib\target\turkcell-bt-java-demo-shaded.jar
```

Demo helper key'leri:

- `BT_EXAMPLE_ACCOUNT=bt.acc.MySystem.MyAccount`
- `BT_EXAMPLE_SAFE_PASSWORD=bt.safe.MyFolder.MyTitle.password`
- `BT_EXAMPLE_SAFE_USERNAME=bt.safe.MyFolder.MyTitle.username`

## Kubernetes

Önerilen manifest setleri:

- `Classic API`: [turkcell-bt-java-lib/k8s/apikey-configmap.yml](turkcell-bt-java-lib/k8s/apikey-configmap.yml), [turkcell-bt-java-lib/k8s/apikey-secret.yml](turkcell-bt-java-lib/k8s/apikey-secret.yml), [turkcell-bt-java-lib/k8s/apikey-deployment.yml](turkcell-bt-java-lib/k8s/apikey-deployment.yml)
- `OAuth`: [turkcell-bt-java-lib/k8s/oauth-configmap.yml](turkcell-bt-java-lib/k8s/oauth-configmap.yml), [turkcell-bt-java-lib/k8s/oauth-secret.yml](turkcell-bt-java-lib/k8s/oauth-secret.yml), [turkcell-bt-java-lib/k8s/oauth-deployment.yml](turkcell-bt-java-lib/k8s/oauth-deployment.yml)

## Operasyon Notları

- Normal kullanımda per-refresh başarı logu basılmaz.
- Daha detaylı log gerekiyorsa geçici olarak `BEYONDTRUST_DEBUG=true` kullanılabilir.
- Refresh ayarı için canonical parametre `BEYONDTRUST_REFRESH_INTERVAL`'dır.
