package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;
import com.turkcell.bt.java.BeyondTrustOptions;

import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
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
                    printSampleValues(snapshot);
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

    private static void printSampleValues(List<Map.Entry<Object, Object>> snapshot) {
        Map<String, String> snapshotMap = snapshot.stream()
                .collect(Collectors.toMap(
                        entry -> entry.getKey().toString(),
                        entry -> entry.getValue() == null ? null : entry.getValue().toString(),
                        (left, right) -> right,
                        LinkedHashMap::new));

        for (String line : buildExampleOutputLines(
                snapshotMap,
                firstConfiguredValue("BT_EXAMPLE_ACCOUNT"),
                firstConfiguredValue("BT_EXAMPLE_SAFE_PASSWORD"),
                firstConfiguredValue("BT_EXAMPLE_SAFE_USERNAME"))) {
            System.out.println(line);
        }
    }

    static List<String> buildExampleOutputLines(
            Map<String, String> snapshot,
            String exampleAccountKey,
            String exampleSafePasswordKey,
            String exampleSafeUsernameKey) {

        List<String> lines = new java.util.ArrayList<>();
        appendExampleOutput(lines, snapshot, "BT_EXAMPLE_ACCOUNT", "example account", "Managed Account Sample", exampleAccountKey);
        appendExampleOutput(lines, snapshot, "BT_EXAMPLE_SAFE_PASSWORD", "example password", "Secret Safe Password Sample", exampleSafePasswordKey);
        appendExampleOutput(lines, snapshot, "BT_EXAMPLE_SAFE_USERNAME", "example username", "Secret Safe Username Sample", exampleSafeUsernameKey);
        return lines;
    }

    private static void appendExampleOutput(
            List<String> lines,
            Map<String, String> snapshot,
            String parameterName,
            String friendlyName,
            String sampleLabel,
            String configuredKey) {

        if (configuredKey == null || configuredKey.isBlank()) {
            lines.add(parameterName + " not set; skipping " + friendlyName + " output");
            return;
        }

        if (!snapshot.containsKey(configuredKey)) {
            lines.add("Demo example key not found: " + configuredKey);
            return;
        }

        lines.add(sampleLabel + " (" + configuredKey + ") = " + snapshot.get(configuredKey));
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
