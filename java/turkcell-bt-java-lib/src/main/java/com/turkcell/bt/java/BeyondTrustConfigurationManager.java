package com.turkcell.bt.java;

import java.util.Map;
import java.util.Properties;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class BeyondTrustConfigurationManager implements AutoCloseable {

    private final BeyondTrustOptions options;
    private final Properties properties;
    private ScheduledExecutorService scheduler;

    public BeyondTrustConfigurationManager(BeyondTrustOptions options) {
        this.options = options;
        this.properties = new Properties();
    }

    // ==========================================================================
    // ⭐ YENİ EKLENEN: STATIC FACTORY METHOD (ZERO CONFIG)
    // Kullanıcı ayar yapmadan direkt bunu çağırır, ortam değişkenlerini otomatik okur.
    // ==========================================================================
    public static BeyondTrustConfigurationManager createAndLoad() {
        // 1. Ortamdan (ConfigMap/Env) ayarları otomatik oku
        BeyondTrustOptions envOptions = BeyondTrustOptions.fromEnv();

        // 2. Manager'ı oluştur
        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(envOptions);

        // 3. Verileri yükle ve servisi başlat
        manager.load();

        return manager;
    }
    // ==========================================================================

    // --- BAŞLATMA VE İLK YÜKLEME ---
    public void load() {
        if (!options.isEnabled()) {
            System.out.println("⚠️ [BeyondTrust] Kutuphane devre disi.");
            return;
        }

        boolean hasUrl = options.getApiUrl() != null && !options.getApiUrl().isBlank();
        boolean oauthReady = options.isUseAppUser()
                && options.getClientId() != null && !options.getClientId().isBlank()
                && options.getClientSecret() != null && !options.getClientSecret().isBlank();
        boolean apiKeyReady = !options.isUseAppUser()
                && options.getApiKey() != null && !options.getApiKey().isBlank();

        if (!hasUrl || (!oauthReady && !apiKeyReady)) {
            System.out.println("⚠️ [BeyondTrust] Konfigurasyon eksik. API_URL ve auth parametrelerini kontrol edin.");
            return;
        }

        // 1. ADIM: Tek Atımlık Yükleme (One-Shot)
        // Süre 0 olsa bile burası çalışır, veriler hafızaya alınır.
        System.out.println("🚀 [BeyondTrust] Başlangıç verileri çekiliyor...");
        loadDataInternal();

        // 2. ADIM: Timer Kontrolü
        long refreshTime = options.getRefreshIntervalSeconds();

        if (refreshTime > 0) {
            startRefreshTimer(refreshTime);
            System.out.println("✅ [BeyondTrust] Otomatik yenileme AKTİF. Periyot: " + refreshTime + " saniye.");
        } else {
            System.out.println("🛑 [BeyondTrust] Otomatik yenileme KAPALI (Süre: 0). Sadece başlangıç verileriyle devam edilecek.");
        }
    }

    private void loadDataInternal() {
        // try-with-resources: Service işi bitince (http client) kapanır, kaynak tüketmez.
        try (BeyondTrustService service = new BeyondTrustService(options)) {
            Map<String, String> newData = service.fetchAllSecrets();

            if (newData != null && !newData.isEmpty()) {
                // Thread-safe olduğu için properties nesnesini güvenle güncelleyebiliriz
                properties.putAll(newData);
            }
        } catch (Exception ex) {
            System.err.println("❌ [BeyondTrust] Veri yükleme hatası: " + ex.getMessage());
        }
    }

    private void startRefreshTimer(long period) {
        scheduler = Executors.newSingleThreadScheduledExecutor(r -> {
            Thread t = new Thread(r, "BeyondTrust-Refresher");
            t.setDaemon(true); // Uygulama kapanırken bu thread engellemesin
            return t;
        });

        scheduler.scheduleAtFixedRate(
                this::loadDataInternal,
                period, // Initial Delay
                period, // Period
                TimeUnit.SECONDS
        );
    }

    // --- VERİ OKUMA ---
    public String getProperty(String key) {
        return properties.getProperty(key);
    }

    public String getProperty(String key, String defaultValue) {
        return properties.getProperty(key, defaultValue);
    }

    public Properties getAllProperties() {
        return properties;
    }

    @Override
    public void close() {
        if (scheduler != null && !scheduler.isShutdown()) {
            scheduler.shutdownNow(); // Bekleyen işleri iptal et
        }
    }
}
