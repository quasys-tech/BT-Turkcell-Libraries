package com.turkcell.bt.java;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import java.util.Properties;
import static org.junit.jupiter.api.Assertions.*;

public class BeyondTrustConfigurationManagerTest {

    @Test
    @DisplayName("Devre dışı bırakıldığında load işlemi erken dönmeli")
    void testLoadProcessDisabled() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(false);

        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options);
        manager.load();

        assertNull(manager.getProperty("any.key"));
        manager.close();
    }

    @Test
    @DisplayName("Property okuma ve varsayılan değer mantığı çalışmalı")
    void testPropertyAccess() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options)) {
            // Manuel olarak veri enjekte edip okumayı test edelim (Coverage için)
            Properties props = manager.getAllProperties();
            props.setProperty("bt.acc.test.password", "secret123");

            // Durum 1: Mevcut anahtarı oku
            assertEquals("secret123", manager.getProperty("bt.acc.test.password"));

            // Durum 2: Mevcut olmayan anahtarı oku (null dönmeli)
            assertNull(manager.getProperty("non.existent"));

            // Durum 3: Mevcut olmayan anahtar için varsayılan değer oku
            assertEquals("default_val", manager.getProperty("non.existent", "default_val"));

            // Durum 4: Mevcut anahtar için varsayılan değer (asıl değeri dönmeli)
            assertEquals("secret123", manager.getProperty("bt.acc.test.password", "default_val"));
        }
    }

    @Test
    @DisplayName("Refresh timer başlatma mantığı kapsanmalı")
    void testStartRefreshTimerLogic() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiKey("test-key");
        // Sadece 1 saniye veriyoruz ki kod startRefreshTimer metodunun içine girsin
        options.setRefreshIntervalSeconds(1);

        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options);

        // Bu çağrı startRefreshTimer() metoduna girecektir
        manager.load();

        assertNotNull(manager.getAllProperties());

        // Scheduler'ın kapatılma (close) mantığını kapsar
        manager.close();
    }

    @Test
    @DisplayName("API Key boş olduğunda yükleme yapılmamalı")
    void testLoadWithEmptyApiKey() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiKey(""); // Boş key

        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options);
        manager.load();

        assertNull(manager.getProperty("any.key"));
        manager.close();
    }
}