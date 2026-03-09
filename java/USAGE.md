# Turkcell BeyondTrust Java Library - USAGE

Bu dokuman, library kurumsal Artifactory'ye deploy edildikten sonra uygulama ekiplerinin
Maven ile paketi nasil import edecegini ve nasil kullanacagini adim adim anlatir.

Not: Asagidaki URL, user, token, repo id degerleri ornektir.

## 1. Gereken Bilgiler

Deploy sonrasi su bilgiler gerekir:

- Artifactory Maven repository URL
- Artifactory kullanici adi veya service account
- Artifactory Access Token/API Key
- Kullanilacak library versiyonu (`<VERSION>`)

Artifact:

- `groupId`: `com.turkcell.bt.java`
- `artifactId`: `turkcell-bt-java-lib`

## 2. `settings.xml` ile Artifactory Yetkilendirme

`~/.m2/settings.xml` dosyasina server bilgisi ekleyin:

```xml
<settings>
  <servers>
    <server>
      <id>turkcell-artifactory</id>
      <username>${env.ARTIFACTORY_USER}</username>
      <password>${env.ARTIFACTORY_TOKEN}</password>
    </server>
  </servers>
</settings>
```

Opsiyonel: Ortam degiskenleri yerine dogrudan deger yazabilirsiniz.

## 3. `pom.xml`'e Repository ve Dependency Ekleme

Consumer proje `pom.xml`:

```xml
<repositories>
  <repository>
    <id>turkcell-artifactory</id>
    <url>https://<ARTIFACTORY_HOST>/artifactory/<MAVEN_REPO></url>
  </repository>
</repositories>

<dependencies>
  <dependency>
    <groupId>com.turkcell.bt.java</groupId>
    <artifactId>turkcell-bt-java-lib</artifactId>
    <version><VERSION></version>
  </dependency>
</dependencies>
```

Kontrol:

```bash
mvn -U clean test
```

## 4. Uygulamada Kullanim

En kolay yol `createAndLoad()`:

```java
import com.turkcell.bt.java.BeyondTrustConfigurationManager;

try (var manager = BeyondTrustConfigurationManager.createAndLoad()) {
    String dbPass = manager.getProperty("bt.acc.Server1.root");
    String apiPass = manager.getProperty("bt.safe.TeamDev.ApiSecret.password");
}
```

Key formatlari:

- Managed Account: `bt.acc.{SystemName}.{AccountName}`
- Secret Safe Password: `bt.safe.{Folder}.{Title}.password`
- Secret Safe Username: `bt.safe.{Folder}.{Title}.username`

## 5. Environment Degiskenlerini Verme

Iki auth modu vardir.

### A) App User (Default/Onerilen)

```env
BEYONDTRUST_ENABLED=true
BEYONDTRUST_API_URL=https://<PAM_HOST>/BeyondTrust/api/public/v3
BEYONDTRUST_USE_APP_USER=true
BEYONDTRUST_CLIENT_ID=<CLIENT_ID>
BEYONDTRUST_CLIENT_SECRET=<CLIENT_SECRET>
BT_REFRESH_TIME=300
BEYONDTRUST_SECRET_SAFE_PATHS=TEAM_DEV,TEAM_TEST
BEYONDTRUST_MANAGED_ACCOUNTS=Server1.root;Server2.admin
BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=false
```

### B) API Key (Legacy)

```env
BEYONDTRUST_ENABLED=true
BEYONDTRUST_API_URL=https://<PAM_HOST>/BeyondTrust/api/public/v3
BEYONDTRUST_USE_APP_USER=false
BEYONDTRUST_API_KEY=key=<API_KEY>; runas=<RUNAS_USER>;
BT_REFRESH_TIME=300
BEYONDTRUST_SECRET_SAFE_PATHS=TEAM_DEV,TEAM_TEST
BEYONDTRUST_MANAGED_ACCOUNTS=Server1.root;Server2.admin
BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=false
```

Not:

- Java tarafinda refresh env anahtari `BT_REFRESH_TIME` olarak kullanilir.
- Env verilmezse default refresh `1800` saniyedir.

## 6. Kubernetes/OpenShift Kullanim Onerisi

- Secret degerlerini (`CLIENT_SECRET`, `API_KEY`) Kubernetes `Secret` icinde tutun.
- Non-sensitive ayarlari `ConfigMap` icine koyun.

Ornek:

```yaml
envFrom:
  - configMapRef:
      name: bt-java-config
  - secretRef:
      name: bt-java-secret
```

## 7. CI/CD Pipeline Onerisi

Consumer projelerde pipeline sirasiyla:

1. `settings.xml` ve credentials enjekte et
2. `mvn -U dependency:resolve`
3. `mvn clean package`
4. Runtime env degiskenlerini deployment asamasinda ver

## 8. Hizli Dogrulama

Calisan uygulamada bilinen bir key ile kontrol edin:

```java
String value = manager.getProperty("bt.acc.Server1.root");
```

Beklenen:

- `null` degilse entegrasyon calisiyor.
- Yenileme araligi sonunda yeni degerler alinabiliyor.

## 9. SIK Hatalar

- `Konfigurasyon eksik`: URL veya secili auth moduna ait env degiskenleri eksik.
- `OAuth access token not found`: Token endpoint cevabi beklenen formatta degil.
- `null` degeri: Key ismi ile hedef account/safe eslesmiyor.

## 10. Guvenlik Notlari

- Secret degerlerini source code icine yazmayin.
- Token ve secret'lari repository'ye commit etmeyin.
- App User icin minimum yetki prensibini uygulayin.
- Duzenli credential rotasyonu yapin.
