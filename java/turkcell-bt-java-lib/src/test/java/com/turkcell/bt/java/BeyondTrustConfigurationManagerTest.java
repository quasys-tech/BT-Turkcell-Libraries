package com.turkcell.bt.java;

import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.lang.reflect.Method;
import java.util.Map;
import java.util.concurrent.atomic.AtomicInteger;

import static org.junit.jupiter.api.Assertions.*;

class BeyondTrustConfigurationManagerTest {

    private static final String[] SYSTEM_PROPERTIES = {
            "BEYONDTRUST_ENABLED",
            "BEYONDTRUST_API_URL",
            "BEYONDTRUST_USE_APP_USER",
            "BEYONDTRUST_API_KEY",
            "BEYONDTRUST_CLIENT_ID",
            "BEYONDTRUST_CLIENT_SECRET",
            "BEYONDTRUST_RUNAS_USER",
            "BEYONDTRUST_IGNORE_SSL_ERRORS",
            "BEYONDTRUST_CERTIFICATE_CONTENT",
            "BEYONDTRUST_REFRESH_INTERVAL",
            "BT_REFRESH_TIME",
            "BEYONDTRUST_MANAGED_ACCOUNTS",
            "BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED",
            "BEYONDTRUST_SECRET_SAFE_PATHS",
            "BEYONDTRUST_ALL_SECRETS_ENABLED"
    };

    @AfterEach
    void tearDown() {
        for (String key : SYSTEM_PROPERTIES) {
            System.clearProperty(key);
        }
    }

    @Test
    @DisplayName("fromEnv system property degerlerini env onceligiyle degil property onceligiyle okumali")
    void fromEnvReadsSystemProperties() {
        System.setProperty("BEYONDTRUST_ENABLED", "true");
        System.setProperty("BEYONDTRUST_API_URL", "https://pam.example.com/BeyondTrust/api/public/v3");
        System.setProperty("BEYONDTRUST_USE_APP_USER", "true");
        System.setProperty("BEYONDTRUST_CLIENT_ID", "client-id");
        System.setProperty("BEYONDTRUST_CLIENT_SECRET", "client-secret");
        System.setProperty("BEYONDTRUST_CERTIFICATE_CONTENT", "pem-content");
        System.setProperty("BEYONDTRUST_REFRESH_INTERVAL", "120");

        BeyondTrustOptions options = BeyondTrustOptions.fromEnv();

        assertTrue(options.isEnabled());
        assertEquals("https://pam.example.com/BeyondTrust/api/public/v3", options.getApiUrl());
        assertTrue(options.isUseAppUser());
        assertTrue(options.isUseAppUserConfigured());
        assertEquals("client-id", options.getClientId());
        assertEquals("client-secret", options.getClientSecret());
        assertEquals("pem-content", options.getCertificateContent());
        assertEquals(120, options.getRefreshIntervalSeconds());
    }

    @Test
    @DisplayName("fromEnv legacy BT_REFRESH_TIME aliasini backward compatibility icin kabul etmeli")
    void fromEnvSupportsLegacyRefreshAlias() {
        System.setProperty("BT_REFRESH_TIME", "45");

        BeyondTrustOptions options = BeyondTrustOptions.fromEnv();

        assertEquals(45, options.getRefreshIntervalSeconds());
    }

    @Test
    @DisplayName("fromEnv explicit false verildiginde classic mode secimini korumali")
    void fromEnvSupportsExplicitClassicMode() {
        System.setProperty("BEYONDTRUST_USE_APP_USER", "false");

        BeyondTrustOptions options = BeyondTrustOptions.fromEnv();

        assertFalse(options.isUseAppUser());
        assertTrue(options.isUseAppUserConfigured());
    }

    @Test
    @DisplayName("Enabled oldugunda BEYONDTRUST_USE_APP_USER eksikse validation error vermeli")
    void validateRequiresExplicitUseAppUserWhenEnabled() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setApiKey("api-key");

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options, Map::of)) {
            assertTrue(invokeValidateRequiredSettings(manager).contains("BEYONDTRUST_USE_APP_USER"));
        }
    }

    @Test
    @DisplayName("OAuth mode secildiginde client id ve secret zorunlu olmali")
    void validateRequiresOAuthCredentialsWhenUseAppUserTrue() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(true);

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options, Map::of)) {
            var missingSettings = invokeValidateRequiredSettings(manager);

            assertTrue(missingSettings.contains("BEYONDTRUST_CLIENT_ID"));
            assertTrue(missingSettings.contains("BEYONDTRUST_CLIENT_SECRET"));
        }
    }

    @Test
    @DisplayName("Classic mode secildiginde api key zorunlu olmali")
    void validateRequiresApiKeyWhenUseAppUserFalse() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options, Map::of)) {
            assertTrue(invokeValidateRequiredSettings(manager).contains("BEYONDTRUST_API_KEY"));
        }
    }

    @Test
    @DisplayName("Devre disi oldugunda manager bos snapshot ile kalmali")
    void loadWhenDisabledKeepsEmptySnapshot() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(false);

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options)) {
            manager.load();
            assertNull(manager.getProperty("bt.acc.anything"));
        }
    }

    @Test
    @DisplayName("Refresh failure son basarili snapshoti korumali")
    void refreshFailureKeepsLastSnapshot() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);
        options.setApiKey("api-key");
        options.setRefreshIntervalSeconds(0);

        AtomicInteger calls = new AtomicInteger();
        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options, () -> {
            if (calls.incrementAndGet() == 1) {
                return Map.of("bt.acc.Sys.Account", "stable-value");
            }

            throw new IllegalStateException("simulated refresh failure");
        })) {
            manager.load();
            invokePrivate(manager, "refreshInternal");

            assertEquals("stable-value", manager.getProperty("bt.acc.Sys.Account"));
        }
    }

    @Test
    @DisplayName("Basarili refresh yeni snapshoti atomik sekilde replace etmeli")
    void refreshSuccessReplacesSnapshotAtomically() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);
        options.setApiKey("api-key");
        options.setRefreshIntervalSeconds(0);

        AtomicInteger calls = new AtomicInteger();
        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options, () -> {
            if (calls.incrementAndGet() == 1) {
                return Map.of(
                        "bt.acc.Sys.Account", "first-value",
                        "bt.safe.Team.Api.password", "first-password");
            }

            return Map.of("bt.acc.Sys.Account", "second-value");
        })) {
            manager.load();
            invokePrivate(manager, "refreshInternal");

            assertEquals("second-value", manager.getProperty("bt.acc.Sys.Account"));
            assertNull(manager.getProperty("bt.safe.Team.Api.password"));
        }
    }

    private static void invokePrivate(Object instance, String methodName) throws Exception {
        Method method = instance.getClass().getDeclaredMethod(methodName);
        method.setAccessible(true);
        method.invoke(instance);
    }

    @SuppressWarnings("unchecked")
    private static java.util.List<String> invokeValidateRequiredSettings(BeyondTrustConfigurationManager manager) throws Exception {
        Method method = BeyondTrustConfigurationManager.class.getDeclaredMethod("validateRequiredSettings");
        method.setAccessible(true);
        return (java.util.List<String>) method.invoke(manager);
    }
}
