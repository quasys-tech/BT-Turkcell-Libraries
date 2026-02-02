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