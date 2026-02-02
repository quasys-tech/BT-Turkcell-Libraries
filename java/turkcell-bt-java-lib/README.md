🚀 Hızlı Başlangıç (Entegrasyon)
1. Bağımlılığı Ekle (Maven)
Projenizin pom.xml dosyasına kütüphaneyi ekleyin (JFrog/Artifactory entegrasyonu sonrası):

<dependency>
    <groupId>com.turkcell.bt.java</groupId>
    <artifactId>turkcell-bt-java-lib</artifactId>
    <version>1.0.0</version>
</dependency>

2. Kullanım (Kod)
Uygulamanızın başlangıcında Manager'ı oluşturun ve şifreleri getProperty ile çağırın:

import com.turkcell.bt.java.BeyondTrustConfigurationManager;

public class App {
    public static void main(String[] args) {
        // Manager'ı başlat (Ayarları ConfigMap'ten otomatik alır)
        try (var btManager = BeyondTrustConfigurationManager.createAndLoad()) {
            
            // Managed Account şifresi çekme
            String dbPass = btManager.getProperty("bt.acc.SystemName.AccountName");
            
            // Secret Safe (Klasör) şifresi çekme
            String apiPass = btManager.getProperty("bt.safe.FolderName.SecretTitle.password");

            System.out.println("Şifre başarıyla alındı: " + dbPass);
        }
    }
}


⚙️ Yapılandırma (OpenShift / Deployment)
Kütüphanenin çalışması için aşağıdaki ortam değişkenlerinin ConfigMap üzerinden pod'a enjekte edilmesi gerekir:

Değişken Adı,Açıklama,Örnek Değer
BEYONDTRUST_API_URL,BeyondTrust API adresi,https://secrets-cache-service/...
BEYONDTRUST_API_KEY,Erişim anahtarı (PS-Auth),key=...; runas=...;
BT_REFRESH_TIME,Yenileme periyodu (saniye),1800 (0 ise yenileme yapmaz)
BEYONDTRUST_MANAGED_ACCOUNTS,İzlenecek hesaplar (;),System1.Acc1;System2.Acc2
BEYONDTRUST_SECRET_SAFE_PATHS,"İzlenecek klasörler (,)","SafeFolder1,SafeFolder2"
BEYONDTRUST_IGNORE_SSL_ERRORS,Sertifika hatası yoksayma,true


🔑 Key Formatı Kuralları
Manager üzerinden şifre çağırırken aşağıdaki formatları kullanmalısınız:

Managed Accounts: bt.acc.[SystemName].[AccountName]

Secret Safe (Şifre): bt.safe.[Folder].[Title].password

Secret Safe (Kullanıcı): bt.safe.[Folder].[Title].username


🛠️ Sorun Giderme
LOGS: Uygulama başladığında 🚀 [BeyondTrust] Başlangıç verileri çekiliyor... logunu gördüğünüzden emin olun.

NULL Hatası: Eğer loglarda BT Error: null görüyorsanız, BEYONDTRUST_API_URL veya API_KEY değerlerinin ConfigMap'te doğru tanımlandığını kontrol edin.

Refresh: Şifrelerin güncellenmesi için BT_REFRESH_TIME değerinin 0'dan büyük olduğundan emin olun.


İpucu: Kütüphane içindeki createAndLoad() metodu AutoCloseable destekler. try-with-resources bloğu içinde kullanmanız, uygulama kapanırken arka plan thread'lerinin temizlenmesini sağlar

Example Configmap  : 

Appliance : 

  BEYONDTRUST_ENABLED: "true"
  BEYONDTRUST_API_URL: "https://pam.quasys.com.tr/BeyondTrust/api/public/v3"
  BEYONDTRUST_API_KEY: "b26a593fdf632aa951d69004f8531d99b5bc53c06c83607ef9d09f711d55a9221890a10cce3ad17af906f389424a6a07028be31fcabf4d1a00dfa21fef72f2f4; runas=enes;"

  # SSL ve Refresh Ayarları
  BEYONDTRUST_IGNORE_SSL_ERRORS: "false"
  BT_REFRESH_TIME: "20"

  # Hangi veriler çekilecek?
  BEYONDTRUST_MANAGED_ACCOUNTS: "dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB;EC2AMAZ-D6OKDG1.deneme"
  BEYONDTRUST_SECRET_SAFE_PATHS: "ENES_SC_DEMO_DEV,ENES_SC_DEMO_TEST"
  BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED: "false"
  BEYONDTRUST_CERTIFICATE_CONTENT: |-
    -----BEGIN CERTIFICATE-----
    MIIGejCCBWKgAwIBAgIQCxP8yr431fBRTbEeSyINlzANBgkqhkiG9w0BAQsFADBg
    MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
    d3cuZGlnaWNlcnQuY29tMR8wHQYDVQQDExZHZW9UcnVzdCBUTFMgUlNBIENBIEcx
    MB4XDTI1MDgwMTAwMDAwMFoXDTI2MDkwMTIzNTk1OVowGjEYMBYGA1UEAwwPKi5x
    dWFzeXMuY29tLnRyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4BWo
    OI6cHZgV3pyvE8upY7Q7QoaIPHBVrdF6osShvYvcFAnstdHVJI/mFYak1JcEcPoA


Example APP : 

package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;

public class POC {

    public static void main(String[] args) {

        System.out.println("🚀 Uygulama Başlatılıyor...");

        // ConfigMap'ten hangi key'leri arayacağımızı okuyoruz
        String safeUserKey = System.getenv("BT_EXAMPLE_SAFE_USERNAME");
        String safePassKey = System.getenv("BT_EXAMPLE_SAFE_PASSWORD");
        String managedAccountKey = System.getenv("BT_EXAMPLE_ACCOUNT");

        try (var manager = BeyondTrustConfigurationManager.createAndLoad()) {

            System.out.println("✅ BeyondTrust Servisi Hazır. İzlenen anahtarlar:");
            System.out.println("👉 Safe User Key: " + safeUserKey);
            System.out.println("👉 Safe Pass Key: " + safePassKey);
            System.out.println("👉 Managed Account: " + managedAccountKey);

            while (true) {
                // ConfigMap'ten gelen key isimlerini kullanarak manager'dan değerleri çekiyoruz
                String exampleUser = manager.getProperty(safeUserKey, "KEY_TANIMSIZ");
                String examplePass = manager.getProperty(safePassKey, "KEY_TANIMSIZ");
                String exampleAcc  = manager.getProperty(managedAccountKey, "KEY_TANIMSIZ");

                System.out.println("\n⏰ Zaman: " + System.currentTimeMillis());
                System.out.println("👤 Safe Username: " + exampleUser);
                System.out.println("🔑 Safe Password: " + examplePass);
                System.out.println("🛡️ Account Pass : " + exampleAcc);
                System.out.println("--------------------------------------------------");

                try {
                    Thread.sleep(5000); 
                } catch (InterruptedException e) {
                    System.out.println("🛑 Uygulama durduruluyor...");
                    break;
                }
            }
        }
    }
}