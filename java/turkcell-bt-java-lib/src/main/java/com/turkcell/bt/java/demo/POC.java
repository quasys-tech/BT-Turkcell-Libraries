package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;

/**
 * ---------------------------------------------------------------------------
 * BEYONDTRUST ENTEGRASYON POC (Proof of Concept)
 * ---------------------------------------------------------------------------
 * Bu sınıf, kütüphanenin "Sıfır Ayar" (Zero Config) özelliği ile nasıl
 * başlatılacağını ve şifrelerin nasıl kullanılacağını gösterir.
 *
 * ÖN KOŞUL:
 * Uygulama çalıştırılmadan önce Kubernetes ConfigMap veya İşletim Sistemi
 * üzerinden aşağıdaki ortam değişkenlerinin set edilmiş olması gerekir:
 * - BEYONDTRUST_API_URL
 * - BEYONDTRUST_API_KEY
 * - BEYONDTRUST_MANAGED_ACCOUNTS
 * - BEYONDTRUST_SECRET_SAFE_PATHS
 * ---------------------------------------------------------------------------
 */
public class POC {

    public static void main(String[] args) {

        System.out.println("🚀 Uygulama Başlatılıyor...");

        // 1. BAŞLAT: createAndLoad() metodu ortam değişkenlerini otomatik okur.
        // try-with-resources bloğu, uygulama kapanırken kaynakları temizler.
        try (var manager = BeyondTrustConfigurationManager.createAndLoad()) {

            System.out.println("✅ BeyondTrust Servisi Hazır. Şifreler izleniyor...");

            // 2. KULLAN: Sonsuz döngü (Gerçek uygulamada burası iş mantığınızdır)
            while (true) {

                // Şifreyi direkt key adıyla istiyoruz.
                // Eğer arka planda refresh süresi (BT_REFRESH_TIME) dolduysa, yeni şifre gelir.
                String dbPass = manager.getProperty("bt.acc.dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB", "BULUNAMADI");
                String apiPass = manager.getProperty("bt.safe.ENES_SC_DEMO_DEV.secret1.password", "BULUNAMADI");

                System.out.println("⏰ Zaman: " + System.currentTimeMillis());
                System.out.println("🔐 DB Pass : " + dbPass);
                System.out.println("🔐 API Pass: " + apiPass);
                System.out.println("--------------------------------------------------");

                try {
                    Thread.sleep(5000); // 5 saniyede bir kontrol
                } catch (InterruptedException e) {
                    System.out.println("🛑 Uygulama durduruluyor...");
                    break;
                }
            }
        }
        // manager.close() burada otomatik çağrılır.
    }
}