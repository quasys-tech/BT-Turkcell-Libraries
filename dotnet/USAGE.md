# Turkcell BeyondTrust .NET Library - USAGE

Bu dokuman, library kurumsal Artifactory'ye deploy edildikten sonra uygulama ekiplerinin
paketi nasil import edecegini ve nasil kullanacagini adim adim anlatir.

Not: Asagidaki URL, user ve token degerleri ornektir. Kendi kurum degerlerinizi kullanin.

## 1. Gereken Bilgiler

Deploy sonrasi su bilgilere sahip olmaniz gerekir:

- Artifactory NuGet feed URL (v3 index)
- Artifactory kullanici adi veya service account
- Artifactory API Key / Access Token
- Kullanilacak paket versiyonu (`<VERSION>`)

Paket:

- `Turkcell.BT.Dotnet.Lib`

## 2. Artifactory NuGet Kaynagi Ekleme

Makinede bir kez su komutu calistirin:

```bash
dotnet nuget add source "https://<ARTIFACTORY_HOST>/artifactory/api/nuget/<NUGET_REPO>" \
  --name "TurkcellArtifactory" \
  --username "<ARTIFACTORY_USER>" \
  --password "<ARTIFACTORY_TOKEN>" \
  --store-password-in-clear-text
```

Kontrol:

```bash
dotnet nuget list source
```

## 3. Paketi Projeye Ekleme

### Yontem A - CLI

```bash
dotnet add package Turkcell.BT.Dotnet.Lib --version <VERSION> --source TurkcellArtifactory
```

### Yontem B - `.csproj`

```xml
<ItemGroup>
  <PackageReference Include="Turkcell.BT.Dotnet.Lib" Version="<VERSION>" />
</ItemGroup>
```

## 4. Uygulamada Aktif Etme

`Program.cs` icinde library provider'i ekleyin:

```csharp
using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddBeyondTrustSecrets();

var app = builder.Build();
```

Bu adimdan sonra sifreler `IConfiguration` uzerinden okunur.

Ornek key formatlari:

- Managed Account: `bt.acc.{SystemName}.{AccountName}`
- Secret Safe Password: `bt.safe.{Folder}.{Title}.password`
- Secret Safe Username: `bt.safe.{Folder}.{Title}.username`

## 5. Environment Degiskenlerini Verme

Iki auth modu vardir.

### A) App User (Onerilen)

```env
BEYONDTRUST_ENABLED=true
BEYONDTRUST_API_URL=https://<PAM_HOST>/BeyondTrust/api/public/v3
BEYONDTRUST_USE_APP_USER=true
BEYONDTRUST_CLIENT_ID=<CLIENT_ID>
BEYONDTRUST_CLIENT_SECRET=<CLIENT_SECRET>
BEYONDTRUST_REFRESH_INTERVAL=300
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
BEYONDTRUST_REFRESH_INTERVAL=300
BEYONDTRUST_SECRET_SAFE_PATHS=TEAM_DEV,TEAM_TEST
BEYONDTRUST_MANAGED_ACCOUNTS=Server1.root;Server2.admin
BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED=false
```

Opsiyonel SSL ayarlari:

```env
BEYONDTRUST_IGNORE_SSL_ERRORS=false
BEYONDTRUST_CERTIFICATE_CONTENT=<PEM_CONTENT>
```

## 6. Kubernetes/OpenShift Kullanim Onerisi

- Secret degerleri (`CLIENT_SECRET`, `API_KEY`) ConfigMap yerine `Secret` icinde tutun.
- ConfigMap sadece non-sensitive ayarlari tasisin.
- Deployment'da envFrom ile ConfigMap + Secret birlikte verin.

Ornek (kisa):

```yaml
envFrom:
  - configMapRef:
      name: bt-dotnet-config
  - secretRef:
      name: bt-dotnet-secret
```

## 7. CI/CD Pipeline Onerisi

Consumer projelerde pipeline sirasiyla:

1. NuGet source ekle/guncelle
2. `dotnet restore`
3. `dotnet build`
4. Runtime env degiskenlerini deployment manifestlerinden ver

## 8. Hizli Dogrulama

Calisan uygulamada bir test key'i okuyun:

```csharp
var dbPass = app.Services.GetRequiredService<IConfiguration>()["bt.acc.Server1.root"];
```

Beklenen:

- Deger `null` degilse entegrasyon calisiyor.
- Refresh suresi dolunca key guncellenebilir.

## 9. SIK Hatalar

- `Konfigurasyon eksik`: Auth ve URL env degiskenleri eksik.
- `SSL connection could not be established`: Sertifika zinciri veya SSL ayari hatali.
- `null/YOK` degeri: Key formati ile BeyondTrust hedef listesi eslesmiyor.

## 10. Guvenlik Notlari

- Secret degerleri kodda hardcode etmeyin.
- Repository'ye gercek token/secret commit etmeyin.
- M2M/App User icin min. yetki prensibi uygulayin.
- Token rotasyonu icin operasyon proseduru tanimlayin.
