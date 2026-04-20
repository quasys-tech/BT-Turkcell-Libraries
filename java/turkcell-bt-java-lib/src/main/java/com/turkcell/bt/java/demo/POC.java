package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;

public final class POC {

    private POC() {
    }

    public static void main(String[] args) throws Exception {
        String exampleAccountKey = System.getenv("BT_EXAMPLE_ACCOUNT").strip();
        String exampleSafePasswordKey = System.getenv("BT_EXAMPLE_SAFE_PASSWORD").strip();
        String exampleSafeUsernameKey = System.getenv("BT_EXAMPLE_SAFE_USERNAME").strip();

        try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
            String previousOutput = "";

            while (true) {
                String currentOutput = """
                        Managed Account Sample (%s) = %s
                        Secret Safe Password Sample (%s) = %s
                        Secret Safe Username Sample (%s) = %s
                        """.formatted(
                        exampleAccountKey, manager.getProperty(exampleAccountKey),
                        exampleSafePasswordKey, manager.getProperty(exampleSafePasswordKey),
                        exampleSafeUsernameKey, manager.getProperty(exampleSafeUsernameKey));

                if (!currentOutput.equals(previousOutput)) {
                    System.out.print(currentOutput);
                    previousOutput = currentOutput;
                }

                Thread.sleep(1000);
            }
        }
    }
}
