# Turkcell BT Java Library

`turkcell-bt-java-lib`, BeyondTrust managed account ve Secret Safe değerlerini refresh destekli bir configuration manager içine yükler.

## Artifactory'den Ekleme

Maven repository örneği:

```xml
<repositories>
  <repository>
    <id>bt-artifactory</id>
    <url>https://<ARTIFACTORY_HOST>/artifactory/<MAVEN_REPO_KEY></url>
  </repository>
</repositories>
```

Dependency örneği:

```xml
<dependency>
  <groupId>com.turkcell.bt.java</groupId>
  <artifactId>turkcell-bt-java-lib</artifactId>
  <version><VERSION></version>
</dependency>
```

## Minimal Entegrasyon

```java
try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String managedPassword = manager.getProperty("bt.acc.MySystem.MyAccount");
    String secretPassword = manager.getProperty("bt.safe.MyFolder.MyTitle.password");
    String secretUsername = manager.getProperty("bt.safe.MyFolder.MyTitle.username");
}
```

## Gerekli Konfigürasyon

- `BEYONDTRUST_ENABLED=true`
- `BEYONDTRUST_API_URL=https://pam.example.com/BeyondTrust/api/public/v3`
- `BEYONDTRUST_USE_APP_USER=true` veya `false`
- `OAuth` için: `BEYONDTRUST_CLIENT_ID` ve `BEYONDTRUST_CLIENT_SECRET`
- `Classic API` için: `BEYONDTRUST_API_KEY` ve gerekirse `BEYONDTRUST_RUNAS_USER`
- Yüklenecek hedefler için: `BEYONDTRUST_MANAGED_ACCOUNTS` ve/veya `BEYONDTRUST_SECRET_SAFE_PATHS`
- Opsiyonel refresh ayarı için: `BEYONDTRUST_REFRESH_INTERVAL`

## Üretilen Key Formatları

- `bt.acc.{SystemName}.{AccountName}`
- `bt.safe.{Folder}.{Title}.password`
- `bt.safe.{Folder}.{Title}.username`

## Notlar

- `BEYONDTRUST_USE_APP_USER` değeri explicit verilmelidir.
- Library başlangıçta snapshot yükler, refresh aktifse arka planda günceller.
- Normal kullanımda per-refresh başarı logu basmaz. Detaylı log gerekiyorsa `BEYONDTRUST_DEBUG=true` kullanılabilir.
- Demo doğrulaması için `pom-demo.xml` içindeki `POC` örneği kullanılabilir.

## Diğer Docs

- [../USAGE.md](../USAGE.md)
- [PARAMETERS.md](PARAMETERS.md)
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
