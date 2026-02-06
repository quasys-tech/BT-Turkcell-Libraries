# 🛡️ Turkcell BeyondTrust PAM Library (.NET)

Bu kütüphane, **BeyondTrust Password Safe** ve **Managed Accounts** servislerinden güvenli bir şekilde şifre (secret) çekmek ve bu şifreleri arka planda otomatik olarak güncel tutmak için geliştirilmiştir.

Uygulama kodunda herhangi bir değişiklik yapmadan, sadece **Ortam Değişkenleri (Environment Variables)** ile yönetilen "Zero-Code Configuration" yapısına sahiptir.

## 🚀 Özellikler

* **Çift Kimlik Doğrulama Desteği:** İster **OAuth2 (App User)**, ister Klasik **API Key** ile çalışabilir.
* **Otomatik Yenileme (Auto-Refresh):** Belirlenen aralıklarla (ör. 5 saniyede bir) şifreleri arka planda günceller.
* **Zero-Config:** `appsettings.json` ile uğraşmadan, doğrudan Kubernetes/Docker ortam değişkenleri ile çalışır.
* **Fail-Safe:** BeyondTrust erişimi kesilse bile, uygulama hafızadaki son başarılı şifre ile çalışmaya devam eder.
* **SSL Yönetimi:** Self-signed sertifikalı ortamlar için esnek SSL yapılandırması.

---

## ⚙️ Konfigürasyon Parametreleri

Kütüphane, aşağıdaki ortam değişkenlerini (Environment Variables) otomatik olarak algılar.

### 1. Temel Ayarlar

| Ortam Değişkeni | Zorunlu? | Varsayılan | Açıklama |
| :--- | :---: | :---: | :--- |
| `BEYONDTRUST_ENABLED` | Hayır | `true` | Kütüphaneyi açıp kapatır. `false` yapılırsa devre dışı kalır. |
| `BEYONDTRUST_API_URL` | **Evet** | - | BeyondTrust API adresi. <br>Örnek: `https://pam.domain.com/BeyondTrust/api/public/v3` |
| `BEYONDTRUST_REFRESH_INTERVAL` | Hayır | `1800` | Şifrelerin kaç saniyede bir yenileneceğini belirler. (Varsayılan: 30 dk). Test için `5` yapılabilir. |

### 2. Kimlik Doğrulama (Authentication)

İki yöntemden **sadece birini** seçmelisiniz. Tavsiye edilen yöntem: **App User (OAuth2)**.

#### A. Yöntem: OAuth2 / App User (Önerilen)

| Ortam Değişkeni | Değer | Açıklama |
| :--- | :--- | :--- |
| `BEYONDTRUST_USE_APP_USER` | `true` | Bu modun aktif olması için mutlaka `true` olmalıdır. |
| `BEYONDTRUST_CLIENT_ID` | `Guid` | BeyondTrust üzerindeki App Registration Client ID. |
| `BEYONDTRUST_CLIENT_SECRET` | `String` | App User için üretilmiş Client Secret. |

#### B. Yöntem: API Key (Legacy)

| Ortam Değişkeni | Değer | Açıklama |
| :--- | :--- | :--- |
| `BEYONDTRUST_USE_APP_USER` | `false` | Varsayılan `false`tur. |
| `BEYONDTRUST_API_KEY` | `Key` | Kullanıcı API Key'i. (Format: `key=...; runas=...;`) |
| `BEYONDTRUST_RUNAS_USER` | `User` | (Opsiyonel) API Key içindeki runas parametresi yerine buradan da verilebilir. |

### 3. Filtreleme ve Hedef Seçimi

Hangi şifrelerin çekileceğini belirler. En az bir tanesi dolu olmalıdır.

| Ortam Değişkeni | Açıklama |
| :--- | :--- |
| `BEYONDTRUST_MANAGED_ACCOUNTS` | Çekilecek Managed Account listesi. Noktalı virgül (`;`) ile ayrılır.<br>Format: `SystemName.AccountName`<br>Örnek: `LinuxServer01.root;DbServer.sa` |
| `BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED` | `true` yapılırsa, yetkili olunan **TÜM** Managed Account'ları çeker. (Dikkatli kullanın!) |
| `BEYONDTRUST_SECRET_SAFE_PATHS` | Çekilecek kasa (Safe) yolları. Klasör veya Secret başlığı olabilir.<br>Örnek: `PROJECT_A_DEV, PROJECT_B_TEST` |

### 4. Güvenlik ve SSL

| Ortam Değişkeni | Varsayılan | Açıklama |
| :--- | :---: | :--- |
| `BEYONDTRUST_IGNORE_SSL_ERRORS` | `false` | `true` yapılırsa SSL sertifika hatalarını yoksayar. (Sadece DEV ortamları için!) |
| `BEYONDTRUST_CERTIFICATE_CONTENT` | - | `.pem` formatındaki sertifika içeriği. Prod ortamlarında güvenli iletişim için kullanılır. |

---

## 💻 Kullanım Örnekleri

### 1. .NET Entegrasyonu (`Program.cs`)

Kütüphaneyi uygulamaya eklemek için tek satır kod yeterlidir:

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Ortam değişkenlerini tarar ve BeyondTrust provider'ı ekler
builder.Configuration.AddBeyondTrustSecrets(); 

var host = builder.Build();

// Şifreye erişim (Standart IConfiguration kullanımı)
var dbPassword = host.Services.GetRequiredService<IConfiguration>()["bt.acc.MyServer.root"];


2. Docker / Kubernetes Environment Örneği
Uygulamanızı deploy ederken kullanacağınız yaml veya env dosyası örneği:

# OAuth2 (App User) Kullanımı
BEYONDTRUST_ENABLED: "true"
BEYONDTRUST_API_URL: "[https://pam.quasys.com.tr/BeyondTrust/api/public/v3](https://pam.quasys.com.tr/BeyondTrust/api/public/v3)"
BEYONDTRUST_REFRESH_INTERVAL: "300" # 5 Dakika

# Auth
BEYONDTRUST_USE_APP_USER: "true"
BEYONDTRUST_CLIENT_ID: "3de4ceb1-bd32-4088-816b-c23eff735d24"
BEYONDTRUST_CLIENT_SECRET: "AOsW+TtZsfx3IvRr0vtYJnnSwDldSv+l1GjZ5jQf03o="

# Hedefler
BEYONDTRUST_MANAGED_ACCOUNTS: "EC2AMAZ-D6OKDG1.deneme"
BEYONDTRUST_SECRET_SAFE_PATHS: "ENES_SC_DEMO_DEV"

# SSL (Test için)
BEYONDTRUST_IGNORE_SSL_ERRORS: "true"


🔑 Key Formatları
Uygulama içerisinde IConfiguration üzerinden verilere şu key formatlarıyla erişilir:

Managed Accounts: bt.acc.{SystemName}.{AccountName}

Örnek: config["bt.acc.Linux01.root"]

Secret Safes: bt.safe.{FolderPath}.{Title}.password bt.safe.{FolderPath}.{Title}.username

Örnek: config["bt.safe.ENES_SC_DEMO_DEV.MySecret.password"]

❓ Sıkça Sorulan Sorular
S: Şifre değiştiğinde uygulamayı restart etmem gerekir mi? C: Hayır. BEYONDTRUST_REFRESH_INTERVAL süresi dolduğunda kütüphane yeni şifreyi otomatik çeker ve IConfiguration nesnesini günceller (IOptionsMonitor tetiklenir).

S: API Key ile OAuth arasındaki fark nedir? C: API Key kullanıcı bazlıdır ve "runas" parametresi gerektirebilir. OAuth (App User) ise uygulama bazlıdır (Client Credentials Flow) ve modern/güvenli olan yöntemdir.

S: 5 saniye refresh süresi sistemi yorar mı? C: Prod ortamında önerilmez. Prod için en az 900sn (15dk) veya 1800sn (30dk) önerilir. 5 saniye sadece test ve PoC çalışmaları içindir.