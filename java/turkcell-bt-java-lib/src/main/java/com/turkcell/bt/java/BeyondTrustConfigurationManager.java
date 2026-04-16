package com.turkcell.bt.java;

import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicReference;

public class BeyondTrustConfigurationManager implements AutoCloseable {

    @FunctionalInterface
    interface SnapshotLoader {
        Map<String, String> load() throws Exception;
    }

    private final BeyondTrustOptions options;
    private final SnapshotLoader snapshotLoader;
    private final AtomicReference<Map<String, String>> snapshot = new AtomicReference<>(Collections.emptyMap());
    private final Object reloadLock = new Object();
    private ScheduledExecutorService scheduler;

    public BeyondTrustConfigurationManager(BeyondTrustOptions options) {
        this(options, () -> {
            try (BeyondTrustService service = new BeyondTrustService(options)) {
                return service.fetchAllSecrets();
            }
        });
    }

    BeyondTrustConfigurationManager(BeyondTrustOptions options, SnapshotLoader snapshotLoader) {
        this.options = options;
        this.snapshotLoader = snapshotLoader;
    }

    public static BeyondTrustConfigurationManager createAndLoad() {
        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(BeyondTrustOptions.fromEnv());
        manager.load();
        return manager;
    }

    public void load() {
        if (!options.isEnabled()) {
            System.out.println("[BeyondTrust] Configuration manager is disabled because BEYONDTRUST_ENABLED=false.");
            return;
        }

        List<String> missingSettings = validateRequiredSettings();
        if (!missingSettings.isEmpty()) {
            System.out.println("[BeyondTrust] Configuration manager was not started because required settings are missing.");
            for (String missingSetting : missingSettings) {
                System.out.println("[BeyondTrust] Missing setting: " + missingSetting);
            }
            return;
        }

        synchronized (reloadLock) {
            if (loadSnapshot("Initial load")) {
                System.out.println("[BeyondTrust] Initial load completed. Loaded " + snapshot.get().size() + " key(s).");
            } else {
                System.out.println("[BeyondTrust] Initial load failed. Keeping empty configuration snapshot.");
            }

            if (options.getRefreshIntervalSeconds() > 0) {
                startRefreshTimer(options.getRefreshIntervalSeconds());
                System.out.println("[BeyondTrust] Background refresh enabled with " + options.getRefreshIntervalSeconds() + "s interval.");
            } else {
                System.out.println("[BeyondTrust] Background refresh is disabled because BEYONDTRUST_REFRESH_INTERVAL=0.");
            }
        }
    }

    public String getProperty(String key) {
        return snapshot.get().get(key);
    }

    public String getProperty(String key, String defaultValue) {
        return snapshot.get().getOrDefault(key, defaultValue);
    }

    public Properties getAllProperties() {
        Properties properties = new Properties();
        properties.putAll(snapshot.get());
        return properties;
    }

    private List<String> validateRequiredSettings() {
        List<String> missingSettings = new ArrayList<>();

        if (options.getApiUrl() == null || options.getApiUrl().isBlank()) {
            missingSettings.add("BEYONDTRUST_API_URL");
        }

        if (!options.isUseAppUserConfigured()) {
            missingSettings.add("BEYONDTRUST_USE_APP_USER");
        } else if (options.isUseAppUser()) {
            if (options.getClientId() == null || options.getClientId().isBlank()) {
                missingSettings.add("BEYONDTRUST_CLIENT_ID");
            }

            if (options.getClientSecret() == null || options.getClientSecret().isBlank()) {
                missingSettings.add("BEYONDTRUST_CLIENT_SECRET");
            }
        } else if (BeyondTrustAuthParsing.parseApiKey(options.getApiKey(), options.getRunAsUser()) == null) {
            missingSettings.add("BEYONDTRUST_API_KEY");
        }

        return missingSettings;
    }

    private void startRefreshTimer(long periodSeconds) {
        scheduler = Executors.newSingleThreadScheduledExecutor(runnable -> {
            Thread thread = new Thread(runnable, "BeyondTrust-Refresher");
            thread.setDaemon(true);
            return thread;
        });

        scheduler.scheduleAtFixedRate(this::refreshInternal, periodSeconds, periodSeconds, TimeUnit.SECONDS);
    }

    private void refreshInternal() {
        synchronized (reloadLock) {
            Map<String, String> previousSnapshot = snapshot.get();
            if (!loadSnapshot("Refresh")) {
                System.out.println("[BeyondTrust] Refresh failed. Keeping the last successful snapshot.");
                return;
            }

            if (previousSnapshot.equals(snapshot.get())) {
                System.out.println("[BeyondTrust] Refresh completed with no snapshot changes.");
            } else {
                System.out.println("[BeyondTrust] Refresh completed. Loaded " + snapshot.get().size() + " key(s).");
            }
        }
    }

    private boolean loadSnapshot(String operation) {
        try {
            Map<String, String> loadedSnapshot = snapshotLoader.load();
            Map<String, String> normalizedSnapshot = new LinkedHashMap<>();
            if (loadedSnapshot != null) {
                normalizedSnapshot.putAll(loadedSnapshot);
            }

            snapshot.set(Collections.unmodifiableMap(normalizedSnapshot));
            return true;
        } catch (Exception ex) {
            System.err.println("[BeyondTrust] " + operation + " failed: " + ex.getMessage());
            return false;
        }
    }

    @Override
    public void close() {
        if (scheduler != null && !scheduler.isShutdown()) {
            scheduler.shutdownNow();
        }
    }
}
