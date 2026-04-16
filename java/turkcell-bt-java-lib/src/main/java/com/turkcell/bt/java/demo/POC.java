package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;

public final class POC {

    private POC() {
    }

    public static void main(String[] args) {
        printMinimalIntegrationExample();

        try (BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad()) {
            String managedPassword = manager.getProperty("bt.acc.<SystemName>.<AccountName>");
            String secretPassword = manager.getProperty("bt.safe.<Folder>.<Title>.password");

            System.out.println("Managed account password lookup result: " + managedPassword);
            System.out.println("Secret safe password lookup result: " + secretPassword);
        }
    }

    public static void printMinimalIntegrationExample() {
        System.out.println("Minimal integration example:");
        System.out.println("""
                BeyondTrustConfigurationManager manager = BeyondTrustConfigurationManager.createAndLoad();
                String managedPassword = manager.getProperty("bt.acc.<SystemName>.<AccountName>");
                String secretPassword = manager.getProperty("bt.safe.<Folder>.<Title>.password");
                """);
        System.out.println();
    }
}
