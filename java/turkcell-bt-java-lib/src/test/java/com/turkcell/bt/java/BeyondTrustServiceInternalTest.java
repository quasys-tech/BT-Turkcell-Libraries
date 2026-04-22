package com.turkcell.bt.java;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.lang.reflect.Constructor;
import java.lang.reflect.Method;
import java.lang.reflect.InvocationTargetException;
import java.net.http.HttpRequest;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

class BeyondTrustServiceInternalTest {

    @Test
    @DisplayName("parseCredentialValue unsupported ve fallback payload sekillerini desteklemeli")
    void parseCredentialValueSupportsAdditionalPayloadShapes() {
        assertEquals("", BeyondTrustService.parseCredentialValue(null));
        assertEquals("", BeyondTrustService.parseCredentialValue("   "));
        assertEquals("secret", BeyondTrustService.parseCredentialValue("{\"Password\":\"secret\"}"));
        assertEquals("raw-secret", BeyondTrustService.parseCredentialValue("raw-secret"));
        assertEquals("{}", BeyondTrustService.parseCredentialValue("{}"));
    }

    @Test
    @DisplayName("parseRequestId unsupported ve fallback payload sekillerini desteklemeli")
    void parseRequestIdSupportsAdditionalPayloadShapes() {
        assertEquals("", BeyondTrustService.parseRequestId(null));
        assertEquals("", BeyondTrustService.parseRequestId("   "));
        assertEquals("", BeyondTrustService.parseRequestId("{}"));
        assertEquals("request-42", BeyondTrustService.parseRequestId("request-42"));
    }

    @Test
    @DisplayName("requestBuilder OAuth token yokken Authorization header yazmamalı")
    void requestBuilderOmitsBearerWhenOAuthTokenIsMissing() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(true);

        BeyondTrustService service = new BeyondTrustService(options, java.net.http.HttpClient.newHttpClient());
        HttpRequest request = service.requestBuilder("Secrets-Safe/Secrets").GET().build();

        assertTrue(request.headers().firstValue("Authorization").isEmpty());
    }

    @Test
    @DisplayName("requestBuilder invalid classic api key oldugunda error vermeli")
    void requestBuilderThrowsForInvalidClassicApiKey() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);
        options.setApiKey("   ");

        BeyondTrustService service = new BeyondTrustService(options, java.net.http.HttpClient.newHttpClient());

        IllegalStateException exception = assertThrows(IllegalStateException.class,
                () -> service.requestBuilder("ManagedAccounts"));

        assertTrue(exception.getMessage().contains("BEYONDTRUST_API_KEY"));
    }

    @Test
    @DisplayName("createSslContext sertifika yoksa null, invalid ise hata donmeli")
    void createSslContextHandlesMissingAndInvalidCertificates() {
        BeyondTrustOptions noCertificate = new BeyondTrustOptions();
        assertNull(BeyondTrustService.createSslContext(noCertificate));

        BeyondTrustOptions invalidCertificate = new BeyondTrustOptions();
        invalidCertificate.setCertificateContent("invalid-pem");

        IllegalStateException exception = assertThrows(IllegalStateException.class,
                () -> BeyondTrustService.createSslContext(invalidCertificate));

        assertTrue(exception.getMessage().contains("Failed to configure BeyondTrust TLS context"));
    }

    @Test
    @DisplayName("private helper metodlar farkli input varyasyonlarini desteklemeli")
    void privateHelpersSupportAdditionalInputShapes() throws Exception {
        assertEquals(List.of("A", "B", "C"), invokeSplitValues("A; B;C", ';'));
        assertEquals(List.of("A", "B"), invokeSplitValues("A,B"));
        assertEquals(List.of(), invokeSplitValues(null));

        assertEquals("https://pam.example.com/BeyondTrust/api/public/v3/",
                invokeNormalizeBaseUrl("https://pam.example.com/BeyondTrust/api/public/v3///"));

        IllegalStateException normalizeException = assertThrows(IllegalStateException.class,
                () -> invokeNormalizeBaseUrl(" "));
        assertTrue(normalizeException.getMessage().contains("BEYONDTRUST_API_URL"));

        ObjectMapper objectMapper = new ObjectMapper();
        var objectNode = objectMapper.readTree("{\"RequestID\":\"123\",\"Other\":\"x\"}");
        var nonNumericNode = objectMapper.readTree("{\"RequestID\":\"abc\"}");

        assertEquals("123", invokeReadValueIgnoreCase(objectNode, "requestid"));
        assertNull(invokeReadValueIgnoreCase(objectMapper.readTree("[]"), "requestid"));
        assertEquals(123, invokeReadIntegerIgnoreCase(objectNode, "requestid"));
        assertEquals(-1, invokeReadIntegerIgnoreCase(nonNumericNode, "requestid"));
        assertEquals(-1, invokeReadIntegerIgnoreCase(objectNode, "missing"));
    }

    @Test
    @DisplayName("findX509TrustManager X509 manager yoksa hata vermeli")
    void findX509TrustManagerThrowsWhenX509ManagerIsMissing() {
        IllegalStateException exception = assertThrows(IllegalStateException.class,
                () -> invokeFindX509TrustManager(new TrustManager[]{new DummyTrustManager()}));

        assertTrue(exception.getMessage().contains("X509TrustManager"));
    }

    @Test
    @DisplayName("CompositeX509TrustManager default ve custom manager arasinda fallback yapabilmeli")
    void compositeX509TrustManagerSupportsFallbackAndMergedIssuers() throws Exception {
        X509Certificate defaultCertificate = dummyCertificate();
        X509Certificate customCertificate = dummyCertificate();

        RecordingX509TrustManager defaultTrustManager = new RecordingX509TrustManager(
                true,
                true,
                new X509Certificate[]{defaultCertificate});

        RecordingX509TrustManager customTrustManager = new RecordingX509TrustManager(
                false,
                false,
                new X509Certificate[]{customCertificate});

        Object composite = newCompositeTrustManager(defaultTrustManager, customTrustManager);

        invokeComposite(composite, "checkClientTrusted");
        invokeComposite(composite, "checkServerTrusted");

        X509Certificate[] acceptedIssuers = (X509Certificate[]) composite.getClass()
                .getDeclaredMethod("getAcceptedIssuers")
                .invoke(composite);

        assertTrue(defaultTrustManager.clientCalled);
        assertTrue(defaultTrustManager.serverCalled);
        assertTrue(customTrustManager.clientCalled);
        assertTrue(customTrustManager.serverCalled);
        assertEquals(2, acceptedIssuers.length);
    }

    @Test
    @DisplayName("default constructor kendi HttpClient instanceini olusturabilmeli")
    void defaultConstructorCreatesHttpClient() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");

        assertDoesNotThrow(() -> {
            try (BeyondTrustService ignored = new BeyondTrustService(options)) {
                assertNotNull(ignored);
            }
        });
    }

    private static List<String> invokeSplitValues(String value, char... separators) throws Exception {
        Method method = BeyondTrustService.class.getDeclaredMethod("splitValues", String.class, char[].class);
        method.setAccessible(true);
        @SuppressWarnings("unchecked")
        List<String> result = (List<String>) method.invoke(null, value, separators);
        return result;
    }

    private static String invokeNormalizeBaseUrl(String value) throws Exception {
        Method method = BeyondTrustService.class.getDeclaredMethod("normalizeBaseUrl", String.class);
        method.setAccessible(true);
        try {
            return (String) method.invoke(null, value);
        } catch (InvocationTargetException exception) {
            if (exception.getCause() instanceof Exception cause) {
                throw cause;
            }
            throw exception;
        }
    }

    private static String invokeReadValueIgnoreCase(Object node, String propertyName) throws Exception {
        Method method = BeyondTrustService.class.getDeclaredMethod("readValueIgnoreCase", com.fasterxml.jackson.databind.JsonNode.class, String.class);
        method.setAccessible(true);
        return (String) method.invoke(null, node, propertyName);
    }

    private static int invokeReadIntegerIgnoreCase(Object node, String propertyName) throws Exception {
        Method method = BeyondTrustService.class.getDeclaredMethod("readIntegerIgnoreCase", com.fasterxml.jackson.databind.JsonNode.class, String.class);
        method.setAccessible(true);
        return (int) method.invoke(null, node, propertyName);
    }

    private static X509TrustManager invokeFindX509TrustManager(TrustManager[] managers) throws Exception {
        Method method = BeyondTrustService.class.getDeclaredMethod("findX509TrustManager", TrustManager[].class);
        method.setAccessible(true);
        try {
            return (X509TrustManager) method.invoke(null, (Object) managers);
        } catch (InvocationTargetException exception) {
            if (exception.getCause() instanceof Exception cause) {
                throw cause;
            }
            throw exception;
        }
    }

    private static Object newCompositeTrustManager(X509TrustManager defaultTrustManager, X509TrustManager customTrustManager) throws Exception {
        Class<?> compositeClass = Class.forName("com.turkcell.bt.java.BeyondTrustService$CompositeX509TrustManager");
        Constructor<?> constructor = compositeClass.getDeclaredConstructor(X509TrustManager.class, X509TrustManager.class);
        constructor.setAccessible(true);
        return constructor.newInstance(defaultTrustManager, customTrustManager);
    }

    private static void invokeComposite(Object composite, String methodName) throws Exception {
        Method method = composite.getClass().getDeclaredMethod(methodName, X509Certificate[].class, String.class);
        method.setAccessible(true);
        method.invoke(composite, new X509Certificate[0], "RSA");
    }

    private static X509Certificate dummyCertificate() {
        return new X509Certificate() {
            @Override public void checkValidity() {}
            @Override public void checkValidity(java.util.Date date) {}
            @Override public int getVersion() { return 3; }
            @Override public java.math.BigInteger getSerialNumber() { return java.math.BigInteger.ONE; }
            @Override public java.security.Principal getIssuerDN() { return () -> "issuer"; }
            @Override public java.security.Principal getSubjectDN() { return () -> "subject"; }
            @Override public java.util.Date getNotBefore() { return new java.util.Date(); }
            @Override public java.util.Date getNotAfter() { return new java.util.Date(); }
            @Override public byte[] getTBSCertificate() { return new byte[0]; }
            @Override public byte[] getSignature() { return new byte[0]; }
            @Override public String getSigAlgName() { return "SHA256withRSA"; }
            @Override public String getSigAlgOID() { return "1.2.840.113549.1.1.11"; }
            @Override public byte[] getSigAlgParams() { return new byte[0]; }
            @Override public boolean[] getIssuerUniqueID() { return new boolean[0]; }
            @Override public boolean[] getSubjectUniqueID() { return new boolean[0]; }
            @Override public boolean[] getKeyUsage() { return new boolean[0]; }
            @Override public int getBasicConstraints() { return -1; }
            @Override public byte[] getEncoded() { return new byte[0]; }
            @Override public void verify(java.security.PublicKey key) {}
            @Override public void verify(java.security.PublicKey key, String sigProvider) {}
            @Override public String toString() { return "dummy-cert"; }
            @Override public java.security.PublicKey getPublicKey() { return null; }
            @Override public boolean hasUnsupportedCriticalExtension() { return false; }
            @Override public java.util.Set<String> getCriticalExtensionOIDs() { return java.util.Set.of(); }
            @Override public java.util.Set<String> getNonCriticalExtensionOIDs() { return java.util.Set.of(); }
            @Override public byte[] getExtensionValue(String oid) { return new byte[0]; }
        };
    }

    private static final class DummyTrustManager implements TrustManager {
    }

    private static final class RecordingX509TrustManager implements X509TrustManager {
        private final boolean failClient;
        private final boolean failServer;
        private final X509Certificate[] acceptedIssuers;
        private boolean clientCalled;
        private boolean serverCalled;

        private RecordingX509TrustManager(boolean failClient, boolean failServer, X509Certificate[] acceptedIssuers) {
            this.failClient = failClient;
            this.failServer = failServer;
            this.acceptedIssuers = acceptedIssuers;
        }

        @Override
        public void checkClientTrusted(X509Certificate[] chain, String authType) throws CertificateException {
            clientCalled = true;
            if (failClient) {
                throw new CertificateException("client fail");
            }
        }

        @Override
        public void checkServerTrusted(X509Certificate[] chain, String authType) throws CertificateException {
            serverCalled = true;
            if (failServer) {
                throw new CertificateException("server fail");
            }
        }

        @Override
        public X509Certificate[] getAcceptedIssuers() {
            return acceptedIssuers;
        }
    }
}
