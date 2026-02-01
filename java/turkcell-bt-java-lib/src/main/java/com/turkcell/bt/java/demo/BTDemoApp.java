package com.turkcell.bt.java.demo;

import com.turkcell.bt.java.BeyondTrustConfigurationManager;
import com.turkcell.bt.java.BeyondTrustOptions;

import java.io.ByteArrayInputStream;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Base64;
import java.util.Properties;
import java.util.TreeMap;

public class BTDemoApp {

    public static void main(String[] args) throws InterruptedException {
        // 1. Ortam Değişkenlerini Ayarla
        setDemoEnvironment();

        // 2. BeyondTrust Ayarlarını Yapılandır
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setApiUrl(System.getProperty("BEYONDTRUST_API_URL"));
        options.setApiKey(System.getProperty("BEYONDTRUST_API_KEY"));
        options.setEnabled(Boolean.parseBoolean(System.getProperty("BEYONDTRUST_ENABLED", "true")));
        options.setSecretSafePaths(System.getProperty("BEYONDTRUST_SECRET_SAFE_PATHS"));
        options.setManagedAccounts(System.getProperty("BEYONDTRUST_MANAGED_ACCOUNTS"));
        options.setAllManagedAccountsEnabled(Boolean.parseBoolean(System.getProperty("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "false")));
        options.setIgnoreSslErrors(Boolean.parseBoolean(System.getProperty("BEYONDTRUST_IGNORE_SSL_ERRORS", "false")));
        options.setRefreshIntervalSeconds(Integer.parseInt(System.getProperty("BT_REFRESH_TIME", "20")));

        // Sertifika İçeriği (İhtiyaç halinde Manager içinde kullanılabilir)
        String certContent = System.getProperty("BEYONDTRUST_CERTIFICATE_CONTENT");
        if (certContent != null && !certContent.isEmpty()) {
            System.out.println("🛡️ Özel sertifika yüklendi. SSL doğrulaması bu sertifika ile yapılacak.");
        }

        // 3. Manager'ı Başlat
        BeyondTrustConfigurationManager manager = new BeyondTrustConfigurationManager(options);
        manager.load();

        System.out.println("🚀 Java Demo Uygulaması Başladı. Şifreler izleniyor...");

        // İlk yüklemede tüm keyleri yazdır
        printAllKeys(manager);

        String lastDbPass = "";
        String lastApiPass = "";
        DateTimeFormatter dtf = DateTimeFormatter.ofPattern("HH:mm:ss");

        // 4. İZLEME DÖNGÜSÜ
        while (true) {
            String currentDbPass = manager.getProperty("bt.acc.dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB", "YOK");
            String currentApiPass = manager.getProperty("bt.safe.ENES_SC_DEMO_DEV.secret1.password", "YOK");

            if (!currentDbPass.equals(lastDbPass) || !currentApiPass.equals(lastApiPass)) {
                System.out.println("\n🔄 [" + dtf.format(LocalDateTime.now()) + "] DEĞİŞİKLİK VEYA İLK YÜKLEME!");
                System.out.println("   📦 DB Pass : " + currentDbPass);
                System.out.println("   📦 API Pass: " + currentApiPass);

                lastDbPass = currentDbPass;
                lastApiPass = currentApiPass;
            } else {
                System.out.print(".");
            }

            Thread.sleep(2000);
        }
    }

    private static void printAllKeys(BeyondTrustConfigurationManager manager) {
        System.out.println("\n--- 🛡️ BEYONDTRUST LOADED KEYS ---");
        Properties props = manager.getAllProperties();
        TreeMap<Object, Object> sortedProps = new TreeMap<>(props);

        if (sortedProps.isEmpty()) {
            System.out.println("⚠️ Henüz hiç BT anahtarı yüklenmemiş.");
        } else {
            sortedProps.forEach((k, v) -> System.out.println("🔑 " + k + " = " + v));
        }
        System.out.println("----------------------------------\n");
    }

    private static void setDemoEnvironment() {
        System.setProperty("BT_REFRESH_TIME", "20");
        System.setProperty("BEYONDTRUST_API_URL", "https://bt-secrets-cache:1858/BeyondTrust/api/public/v3");
        //System.setProperty("BEYONDTRUST_API_URL", "https://pam.quasys.com.tr/BeyondTrust/api/public/v3");
        System.setProperty("BEYONDTRUST_API_KEY", "b26a593fdf632aa951d69004f8531d99b5bc53c06c83607ef9d09f711d55a9221890a10cce3ad17af906f389424a6a07028be31fcabf4d1a00dfa21fef72f2f4; runas=enes;");
        System.setProperty("BEYONDTRUST_ENABLED", "true");
        System.setProperty("BEYONDTRUST_SECRET_SAFE_PATHS", "ENES_SC_DEMO_DEV,ENES_SC_DEMO_TEST");
        System.setProperty("BEYONDTRUST_MANAGED_ACCOUNTS", "dnsname (Db Instance: dbname, Port:1521).MA_EMPTYDB;EC2AMAZ-D6OKDG1.deneme");
        System.setProperty("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", "true");
        System.setProperty("BEYONDTRUST_IGNORE_SSL_ERRORS", "false"); // Sertifika eklendiği için artık false yapabiliriz
// String cert değişkenini güncelledik:
        String cert = "-----BEGIN CERTIFICATE-----\n" +
                "MIIC3jCCAcagAwIBAgIQXZMK5560pL1EBpZwOrVuuTANBgkqhkiG9w0BAQsFADAY\n" +
                "MRYwFAYDVQQDEw1CVFNFQ1JFVENBQ0hFMB4XDTI2MDEyNjA5MjUwOFoXDTI2MDcy\n" +
                "ODA5MjUwOFowGDEWMBQGA1UEAxMNQlRTRUNSRVRDQUNIRTCCASIwDQYJKoZIhvcN\n" +
                "AQEBBQADggEPADCCAQoCggEBAKNdN5hiEXiEbl6OkSaed2xAtXG5ATgu0GbWjk+s\n" +
                "OQWPY5DkmoJR93+5QHxPYkGVrcuaEkhH/WIHmJzm/eWA0fZ9fBfCiQj1CC/GQJtY\n" +
                "5FMZo2++0IOdWMI7PMUG8FU3H2bQDNZ9Fy1duD6+7Gcs3us5w32q7UsPvqBsr4gJ\n" +
                "eyUHvPyE5583kXCHCmj0bsa3HocOu0lLtL5XcHSfnQxFgLl3ZGGyfzX0PeipKJ7o\n" +
                "V9vELD51XV8AnwQnLphxjYnLzNmlzu3xDxL083E8+oPUn+7fYRyt9qXXusMWjHbJ\n" +
                "5a8QBameycBa2wgQu6Iwp6DAsyH0Bg9MBCfCJ0UB3oiW9GUCAwEAAaMkMCIwEwYD\n" +
                "VR0lBAwwCgYIKwYBBQUHAwEwCwYDVR0PBAQDAgQwMA0GCSqGSIb3DQEBCwUAA4IB\n" +
                "AQCL/xKIphMG5LveETNRtetTXH9w83wyeO/b8sJCuY20e8BpaoE4lUqizLKEHwk8\n" +
                "lLN/suQIGanAGbJa35iojGjoytY004k9qDlki0jW2k7gGTk1Jd8B4IQEMZN7EKeE\n" +
                "XeSIJWUG5TT7YoJKInprnoh7Yb12H7hcDj7abUBBkcL3Guw5hTSMjWKnRuLw3NTX\n" +
                "9Bwk45ZQVv4Vt3JW13st+DBjJpoZ11vOW8rlxv4NeL/VNJxObq0bPfiTodpzx6gd\n" +
                "kxH40iLUD2eCofZiuCLioz/RdW0Sdop0uVonA7KBRXaU1GIYSLxwtjUhbFwtJf5s\n" +
                "jsxeHGU0jJK6c3KwY0wIr6Wi\n" +
                "-----END CERTIFICATE-----";
        // SERTİFİKA İÇERİĞİ==pam.quasys.com.tr
//        String cert = "-----BEGIN CERTIFICATE-----\n" +
//                "MIIGejCCBWKgAwIBAgIQCxP8yr431fBRTbEeSyINlzANBgkqhkiG9w0BAQsFADBg\n" +
//                "MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3\n" +
//                "d3cuZGlnaWNlcnQuY29tMR8wHQYDVQQDExZHZW9UcnVzdCBUTFMgUlNBIENBIEcx\n" +
//                "MB4XDTI1MDgwMTAwMDAwMFoXDTI2MDkwMTIzNTk1OVowGjEYMBYGA1UEAwwPKi5x\n" +
//                "dWFzeXMuY29tLnRyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA4BWo\n" +
//                "OI6cHZgV3pyvE8upY7Q7QoaIPHBVrdF6osShvYvcFAnstdHVJI/mFYak1JcEcPoA\n" +
//                "g4RQImh4S/92K184+WPmJbpbAEQqcQv+r/h64dVWQ2Nk1TxgzrBT6o8lYz2Ai28h\n" +
//                "68qpAyJNFD/f1FS6k7gsw7kGfAv0xKQrYAKq40r6zQ2QqlZ+Jmx19gvelvhNPBs+\n" +
//                "HTtvXT6cW11U02CsWTaosMZHPJhRk0Q08nljV3q7ju/Aexaw+cKAWRkQiTkVNiWO\n" +
//                "AAutpUKL3rpr2RTkP6EgAUZWKe1d/fWiPEfxKvfB85zKAYZdCGxno33i1ckgxDZD\n" +
//                "4ZivjrX5nfsQFvN60QIDAQABo4IDdDCCA3AwHwYDVR0jBBgwFoAUlE/UXYvkpOKm\n" +
//                "gP792PkA76O+AlcwHQYDVR0OBBYEFALcy71u0QtGmOEV06D7RyeAvQHqMHYGA1Ud\n" +
//                "EQRvMG2CDyoucXVhc3lzLmNvbS50coIMcXVhc3lzZHguY29tghB3d3cucXVhc3lz\n" +
//                "ZHguY29tgglxdWFzeXMuYXqCDXd3dy5xdWFzeXMuYXqCEXd3dy5xdWFzeXMuY29t\n" +
//                "LnRygg1xdWFzeXMuY29tLnRyMD4GA1UdIAQ3MDUwMwYGZ4EMAQIBMCkwJwYIKwYB\n" +
//                "BQUHAgEWG2h0dHA6Ly93d3cuZGlnaWNlcnQuY29tL0NQUzAOBgNVHQ8BAf8EBAMC\n" +
//                "BaAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMD8GA1UdHwQ4MDYwNKAy\n" +
//                "oDCGLmh0dHA6Ly9jZHAuZ2VvdHJ1c3QuY29tL0dlb1RydXN0VExTUlNBQ0FHMS5j\n" +
//                "cmwwdgYIKwYBBQUHAQEEajBoMCYGCCsGAQUFBzABhhpodHRwOi8vc3RhdHVzLmdl\n" +
//                "b3RydXN0LmNvbTA+BggrBgEFBQcwAoYyaHR0cDovL2NhY2VydHMuZ2VvdHJ1c3Qu\n" +
//                "Y29tL0dlb1RydXN0VExTUlNBQ0FHMS5jcnQwDAYDVR0TAQH/BAIwADCCAX4GCisG\n" +
//                "AQQB1nkCBAIEggFuBIIBagFoAHcA1219ENGn9XfCx+lf1wC/+YLJM1pl4dCzAXMX\n" +
//                "wMjFaXcAAAGYZUoDfQAABAMASDBGAiEAiO1MUH4OTH/+JDoK1bnKoKgFslozb3kb\n" +
//                "ZEQVwUpqlUQCIQD18oLagPjqQdig+8fzNTYrj3sXxVJrZmzj4yTVKFm4hQB2AMIx\n" +
//                "fldFGaNF7n843rKQQevHwiFaIr9/1bWtdprZDlLNAAABmGVKA+AAAAQDAEcwRQIg\n" +
//                "Z69/Wd9T7LP6bigvBTxdrRgTZLtLL/huc0P4sk/OvtQCIQD/CG8LIK1Mq6eEBFhS\n" +
//                "KU31XbEE18iqhy4xSyGEHcxd4QB1AJROQ4f67MHvgfMZJCaoGGUBx9NfOAIBP3Jn\n" +
//                "fVU3LhnYAAABmGVKA80AAAQDAEYwRAIgBcLbTxIyOGhBAMZkToCZ47QucIVoCjUG\n" +
//                "FCH9p7N0kysCIEbKcLBXPbxk24loPqcHP1dvIjWOD1jNXdud0fefwvhDMA0GCSqG\n" +
//                "SIb3DQEBCwUAA4IBAQBzCRwS1aMZchuJuJ4ybNzSpPCtYVeZGQY+R8QEohojl97Y\n" +
//                "4zKSk8My7dNu0Z4x0VVHIRZ6Cw2jmvVy9JgwfNlx643E0BJUeWjOm95DcD9oCo9o\n" +
//                "YIm/eEqAW8k7QonwMTp5QWV/fRzEvkqATnwGiKaXtYzuhHT3biLtq28fp8Bucwai\n" +
//                "NOK3y2332ZOkBzEbvvlkOE/H56b2tagGM0JTaLGo2Y/osBVJOqOdGvlv/4BAzaFr\n" +
//                "Wjj24i1gAJodXGhRCcwIT2q7k3iTVnZIs6uPM38iHTWbzcqIUB1TZ3OvhbSivfYu\n" +
//                "E4jfa+lVJQnYxL9Yu+tLrzlhn6EpzrrP6+p8ewA6\n" +
//                "-----END CERTIFICATE-----";
        System.setProperty("BEYONDTRUST_CERTIFICATE_CONTENT", cert);
    }
}