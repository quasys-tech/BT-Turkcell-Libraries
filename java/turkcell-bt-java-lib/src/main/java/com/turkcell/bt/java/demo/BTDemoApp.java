package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;
import com.turkcell.bt.java.BeyondTrustOptions;

import java.util.Comparator;
import java.util.List;
import java.util.Map;
import java.util.Properties;
import java.util.stream.Collectors;

public final class BTDemoApp {

    private BTDemoApp() {
    }

    public static void main(String[] args) throws Exception {
        BeyondTrustOptions options = BeyondTrustOptions.fromEnv();

        printBanner(options);
        POC.printMinimalIntegrationExample();

        try (BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options)) {
            manager.load();

            String previousSnapshotHash = "";

            do {
                List<Map.Entry<Object, Object>> snapshot = manager.getAllProperties().entrySet().stream()
                        .filter(entry -> entry.getKey().toString().startsWith("bt."))
                        .sorted(Comparator.comparing(entry -> entry.getKey().toString(), String.CASE_INSENSITIVE_ORDER))
                        .collect(Collectors.toList());

                String currentSnapshotHash = snapshot.stream()
                        .map(entry -> entry.getKey() + "=" + entry.getValue())
                        .collect(Collectors.joining("|"));

                if (!currentSnapshotHash.equals(previousSnapshotHash)) {
                    System.out.println();
                    System.out.println("[" + java.time.LocalTime.now().withNano(0) + "] Snapshot updated. " + snapshot.size() + " BeyondTrust key(s) loaded.");
                    printAllKeys(snapshot);
                    printSampleValues(options, manager, snapshot);
                    previousSnapshotHash = currentSnapshotHash;
                }

                if (options.getRefreshIntervalSeconds() <= 0) {
                    break;
                }

                Thread.sleep(2000);
            } while (true);
        }
    }

    private static void printBanner(BeyondTrustOptions options) {
        System.out.println("============================================================");
        System.out.println("DEMO ONLY - RAW SECRET LOGGING ENABLED - DO NOT USE THIS LOGGING STYLE IN PRODUCTION");
        System.out.println("============================================================");
        System.out.println("Auth Mode : " + (options.isUseAppUser() ? "OAuth / App User" : "Classic API"));
        System.out.println("API Url   : " + (options.getApiUrl() == null || options.getApiUrl().isBlank() ? "<not configured>" : options.getApiUrl()));
        System.out.println("Refresh   : " + options.getRefreshIntervalSeconds() + " second(s)");
        System.out.println();
    }

    private static void printAllKeys(List<Map.Entry<Object, Object>> snapshot) {
        System.out.println("--- Loaded bt.* keys ---");

        if (snapshot.isEmpty()) {
            System.out.println("No BeyondTrust keys are currently available. Check the required environment variables and API connectivity.");
        } else {
            for (Map.Entry<Object, Object> entry : snapshot) {
                System.out.println(entry.getKey() + " = " + entry.getValue());
            }
        }

        System.out.println("------------------------");
    }

    private static void printSampleValues(
            BeyondTrustOptions options,
            BeyondTrustConfigurationManager manager,
            List<Map.Entry<Object, Object>> snapshot) {

        String sampleManagedAccountKey = resolveExampleKey("BT_EXAMPLE_ACCOUNT", snapshot, "bt.acc.", null);
        String sampleSecretSafeKey = resolveExampleKey("BT_EXAMPLE_SAFE_PASSWORD", snapshot, "bt.safe.", ".password");

        if (sampleManagedAccountKey != null) {
            System.out.println("Managed Account Sample (" + sampleManagedAccountKey + ") = " + manager.getProperty(sampleManagedAccountKey));
        }

        if (sampleSecretSafeKey != null) {
            System.out.println("Secret Safe Password Sample (" + sampleSecretSafeKey + ") = " + manager.getProperty(sampleSecretSafeKey));
        }
    }

    private static String resolveExampleKey(
            String propertyName,
            List<Map.Entry<Object, Object>> snapshot,
            String prefix,
            String suffix) {

        String configuredValue = firstConfiguredValue(propertyName);
        if (configuredValue != null && !configuredValue.isBlank()) {
            return configuredValue;
        }

        for (Map.Entry<Object, Object> entry : snapshot) {
            String key = entry.getKey().toString();
            if (key.startsWith(prefix) && (suffix == null || key.endsWith(suffix))) {
                return key;
            }
        }

        return null;
    }

    private static String firstConfiguredValue(String key) {
        String systemProperty = System.getProperty(key);
        if (systemProperty != null && !systemProperty.isBlank()) {
            return systemProperty;
        }

        String environmentValue = System.getenv(key);
        return environmentValue != null && !environmentValue.isBlank() ? environmentValue : null;
    }
}
