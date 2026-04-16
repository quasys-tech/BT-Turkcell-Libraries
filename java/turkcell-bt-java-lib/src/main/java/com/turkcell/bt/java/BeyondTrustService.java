package com.turkcell.bt.java;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.json.JsonMapper;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.TrustManagerFactory;
import javax.net.ssl.X509TrustManager;
import java.io.ByteArrayInputStream;
import java.net.CookieManager;
import java.net.CookiePolicy;
import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.security.KeyStore;
import java.security.SecureRandom;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.time.Duration;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class BeyondTrustService implements AutoCloseable {

    private static final Duration REQUEST_TIMEOUT = Duration.ofSeconds(30);

    private final BeyondTrustOptions options;
    private final HttpClient httpClient;
    private final ObjectMapper objectMapper;
    private String bearerToken;

    public BeyondTrustService(BeyondTrustOptions options) {
        this(options, createHttpClient(options));
    }

    public BeyondTrustService(BeyondTrustOptions options, HttpClient httpClient) {
        this.options = options;
        this.httpClient = httpClient;
        this.objectMapper = JsonMapper.builder()
                .configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true)
                .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
                .build();
    }

    public Map<String, String> fetchAllSecrets() {
        try {
            Map<String, String> snapshot = new LinkedHashMap<>();
            authenticate();

            if (options.isAllManagedAccountsEnabled() || hasValue(options.getManagedAccounts())) {
                processManagedAccounts(snapshot);
            }

            if (hasValue(options.getSecretSafePaths())) {
                processSecretSafe(snapshot);
            }

            if (options.isAllSecretsEnabled()) {
                System.out.println("[BeyondTrust] BEYONDTRUST_ALL_SECRETS_ENABLED is accepted for compatibility, but Secret Safe loading still uses BEYONDTRUST_SECRET_SAFE_PATHS.");
            }

            return snapshot;
        } catch (Exception ex) {
            throw new IllegalStateException("BeyondTrust secret loading failed: " + ex.getMessage(), ex);
        }
    }

    HttpRequest.Builder requestBuilder(String path) {
        HttpRequest.Builder builder = HttpRequest.newBuilder()
                .uri(URI.create(normalizeBaseUrl(options.getApiUrl()) + path))
                .timeout(REQUEST_TIMEOUT);

        if (options.isUseAppUser()) {
            if (hasValue(bearerToken)) {
                builder.header("Authorization", "Bearer " + bearerToken);
            }
            return builder;
        }

        BeyondTrustAuthParsing.ParsedApiKey parsedApiKey =
                BeyondTrustAuthParsing.parseApiKey(options.getApiKey(), options.getRunAsUser());

        if (parsedApiKey == null) {
            throw new IllegalStateException("Classic API authentication requires a valid BEYONDTRUST_API_KEY value.");
        }

        return builder.header("Authorization", parsedApiKey.toAuthorizationHeader());
    }

    static String parseRequestId(String payload) {
        if (payload == null || payload.isBlank()) {
            return "";
        }

        String trimmed = payload.trim();
        try {
            JsonNode root = new ObjectMapper().readTree(trimmed);
            if (root.isNumber() || root.isTextual()) {
                return root.asText();
            }

            String requestId = readValueIgnoreCase(root, "RequestID");
            return requestId == null ? "" : requestId;
        } catch (Exception ignored) {
            return trimmed.replace("\"", "");
        }
    }

    static String parseCredentialValue(String payload) {
        if (payload == null || payload.isBlank()) {
            return "";
        }

        String trimmed = payload.trim();
        try {
            JsonNode root = new ObjectMapper().readTree(trimmed);
            if (root.isTextual()) {
                return root.asText();
            }

            String credential = readValueIgnoreCase(root, "Credential");
            if (credential != null) {
                return credential;
            }

            String password = readValueIgnoreCase(root, "Password");
            if (password != null) {
                return password;
            }
        } catch (Exception ignored) {
            return trimmed.replace("\"", "");
        }

        return trimmed.replace("\"", "");
    }

    static SSLContext createSslContext(BeyondTrustOptions options) {
        try {
            if (options.isIgnoreSslErrors()) {
                return trustAllSslContext();
            }

            if (!hasValue(options.getCertificateContent())) {
                return null;
            }

            X509TrustManager defaultTrustManager = getDefaultTrustManager();
            X509TrustManager customTrustManager = getCustomTrustManager(options.getCertificateContent());
            X509TrustManager compositeTrustManager = new CompositeX509TrustManager(defaultTrustManager, customTrustManager);

            SSLContext sslContext = SSLContext.getInstance("TLS");
            sslContext.init(null, new TrustManager[]{compositeTrustManager}, new SecureRandom());
            return sslContext;
        } catch (Exception ex) {
            throw new IllegalStateException("Failed to configure BeyondTrust TLS context: " + ex.getMessage(), ex);
        }
    }

    private void authenticate() throws Exception {
        if (options.isUseAppUser()) {
            loginWithOAuth();
            return;
        }

        loginWithApiKey();
    }

    private void loginWithOAuth() throws Exception {
        bearerToken = null;

        String formBody = "grant_type=client_credentials"
                + "&client_id=" + URLEncoder.encode(options.getClientId() == null ? "" : options.getClientId(), StandardCharsets.UTF_8)
                + "&client_secret=" + URLEncoder.encode(options.getClientSecret() == null ? "" : options.getClientSecret(), StandardCharsets.UTF_8);

        HttpRequest tokenRequest = HttpRequest.newBuilder()
                .uri(URI.create(normalizeBaseUrl(options.getApiUrl()) + "Auth/Connect/Token"))
                .timeout(REQUEST_TIMEOUT)
                .header("Authorization", "PS-Auth")
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(formBody))
                .build();

        HttpResponse<String> tokenResponse = httpClient.send(tokenRequest, HttpResponse.BodyHandlers.ofString());
        ensureSuccess(tokenResponse.statusCode(), "OAuth token request");

        JsonNode tokenPayload = objectMapper.readTree(tokenResponse.body() == null ? "" : tokenResponse.body());
        String accessToken = readValueIgnoreCase(tokenPayload, "access_token");
        if (accessToken == null || accessToken.isBlank()) {
            throw new IllegalStateException("OAuth token response did not contain access_token.");
        }

        bearerToken = accessToken;
        postSignAppIn();
    }

    private void loginWithApiKey() throws Exception {
        bearerToken = null;
        BeyondTrustAuthParsing.ParsedApiKey parsedApiKey =
                BeyondTrustAuthParsing.parseApiKey(options.getApiKey(), options.getRunAsUser());

        if (parsedApiKey == null) {
            throw new IllegalStateException("Classic API authentication requires a valid BEYONDTRUST_API_KEY value.");
        }

        postSignAppIn();
    }

    private void postSignAppIn() throws Exception {
        HttpResponse<String> response = httpClient.send(
                requestBuilder("Auth/SignAppin")
                        .header("Content-Type", "application/json")
                        .POST(HttpRequest.BodyPublishers.ofString("{}"))
                        .build(),
                HttpResponse.BodyHandlers.ofString());

        ensureSuccess(response.statusCode(), "Auth/SignAppin");
    }

    private void processManagedAccounts(Map<String, String> snapshot) throws Exception {
        HttpResponse<String> response = httpClient.send(
                requestBuilder("ManagedAccounts").GET().build(),
                HttpResponse.BodyHandlers.ofString());

        ensureSuccess(response.statusCode(), "ManagedAccounts");

        List<ManagedAccountDto> accounts = objectMapper.readValue(
                response.body() == null ? "[]" : response.body(),
                new TypeReference<List<ManagedAccountDto>>() {});

        for (ManagedAccountDto account : filterAccounts(accounts)) {
            String configKey = "bt.acc." + account.getSystemName().trim() + "." + account.getAccountName().trim();
            snapshot.put(configKey, fetchManagedAccountPassword(account.getSystemId(), account.getAccountId()));
        }
    }

    private String fetchManagedAccountPassword(int systemId, int accountId) throws Exception {
        String requestId = "";

        try {
            String payload = objectMapper.writeValueAsString(Map.of(
                    "systemId", systemId,
                    "accountId", accountId,
                    "durationMinutes", 5,
                    "reason", "TurkcellAutoFetch"));

            HttpResponse<String> createResponse = httpClient.send(
                    requestBuilder("Requests")
                            .header("Content-Type", "application/json")
                            .POST(HttpRequest.BodyPublishers.ofString(payload))
                            .build(),
                    HttpResponse.BodyHandlers.ofString());

            if (isSuccess(createResponse.statusCode())) {
                requestId = parseRequestId(createResponse.body());
            } else if (createResponse.statusCode() == 409 || createResponse.statusCode() == 403) {
                requestId = findExistingRequestId(systemId, accountId);
            } else {
                throw new IllegalStateException("Request creation failed with status " + createResponse.statusCode() + ".");
            }

            if (!hasValue(requestId)) {
                throw new IllegalStateException("Request ID could not be resolved for the managed account credential flow.");
            }

            for (int attempt = 0; attempt < 5; attempt++) {
                HttpResponse<String> credentialResponse = httpClient.send(
                        requestBuilder("Credentials/" + URLEncoder.encode(requestId, StandardCharsets.UTF_8))
                                .GET()
                                .build(),
                        HttpResponse.BodyHandlers.ofString());

                if (isSuccess(credentialResponse.statusCode())) {
                    return parseCredentialValue(credentialResponse.body());
                }

                Thread.sleep(Duration.ofSeconds(attempt + 1).toMillis());
            }

            throw new IllegalStateException("Credential retrieval failed for RequestID '" + requestId + "'.");
        } finally {
            if (hasValue(requestId)) {
                tryCheckIn(requestId);
            }
        }
    }

    private void tryCheckIn(String requestId) {
        try {
            HttpResponse<String> response = httpClient.send(
                    requestBuilder("Requests/" + URLEncoder.encode(requestId, StandardCharsets.UTF_8) + "/Checkin")
                            .header("Content-Type", "application/json")
                            .PUT(HttpRequest.BodyPublishers.ofString("{\"reason\":\"Done\"}"))
                            .build(),
                    HttpResponse.BodyHandlers.ofString());

            if (!isSuccess(response.statusCode())) {
                System.out.println("[BeyondTrust] Check-in failed for RequestID '" + requestId + "' with status " + response.statusCode() + ".");
            }
        } catch (Exception ex) {
            System.out.println("[BeyondTrust] Check-in failed for RequestID '" + requestId + "': " + ex.getMessage());
        }
    }

    private String findExistingRequestId(int systemId, int accountId) throws Exception {
        HttpResponse<String> response = httpClient.send(
                requestBuilder("Requests").GET().build(),
                HttpResponse.BodyHandlers.ofString());

        ensureSuccess(response.statusCode(), "Requests lookup");

        JsonNode root = objectMapper.readTree(response.body() == null ? "[]" : response.body());
        if (!root.isArray()) {
            return "";
        }

        for (JsonNode item : root) {
            int currentSystemId = readIntegerIgnoreCase(item, "SystemID");
            int currentAccountId = readIntegerIgnoreCase(item, "AccountID");

            if (currentSystemId == systemId && currentAccountId == accountId) {
                String requestId = readValueIgnoreCase(item, "RequestID");
                return requestId == null ? "" : requestId;
            }
        }

        return "";
    }

    private void processSecretSafe(Map<String, String> snapshot) throws Exception {
        for (String path : splitValues(options.getSecretSafePaths())) {
            HttpResponse<String> response = httpClient.send(
                    requestBuilder("Secrets-Safe/Secrets?Path=" + URLEncoder.encode(path, StandardCharsets.UTF_8))
                            .GET()
                            .build(),
                    HttpResponse.BodyHandlers.ofString());

            ensureSuccess(response.statusCode(), "Secrets-Safe for path '" + path + "'");

            List<SecretSafeItemDto> items = objectMapper.readValue(
                    response.body() == null ? "[]" : response.body(),
                    new TypeReference<List<SecretSafeItemDto>>() {});

            for (SecretSafeItemDto item : items) {
                String folder = hasValue(item.getFolder()) ? item.getFolder().trim() : path;
                String title = hasValue(item.getTitle()) ? item.getTitle().trim() : "Untitled";
                String baseKey = "bt.safe." + folder + "." + title;

                snapshot.put(baseKey + ".password", item.getPassword() == null ? "" : item.getPassword());

                String username = hasValue(item.getUsername())
                        ? item.getUsername().trim()
                        : (hasValue(item.getAccount()) ? item.getAccount().trim() : null);

                if (hasValue(username)) {
                    snapshot.put(baseKey + ".username", username);
                }
            }
        }
    }

    private List<ManagedAccountDto> filterAccounts(List<ManagedAccountDto> allAccounts) {
        if (options.isAllManagedAccountsEnabled()) {
            return allAccounts;
        }

        List<String> requestedAccounts = splitValues(options.getManagedAccounts(), ';');
        if (requestedAccounts.isEmpty()) {
            return Collections.emptyList();
        }

        Set<String> requestedAccountSet = new HashSet<>(requestedAccounts);
        List<ManagedAccountDto> filteredAccounts = new ArrayList<>();

        for (ManagedAccountDto account : allAccounts) {
            String key = account.getSystemName().trim() + "." + account.getAccountName().trim();
            if (requestedAccountSet.contains(key)) {
                filteredAccounts.add(account);
            }
        }

        Set<String> returnedAccountSet = new HashSet<>();
        for (ManagedAccountDto account : filteredAccounts) {
            returnedAccountSet.add(account.getSystemName().trim() + "." + account.getAccountName().trim());
        }

        for (String requestedAccount : requestedAccountSet) {
            if (!returnedAccountSet.contains(requestedAccount)) {
                System.out.println("[BeyondTrust] Managed account was requested but not returned by the API: " + requestedAccount);
            }
        }

        return filteredAccounts;
    }

    private static HttpClient createHttpClient(BeyondTrustOptions options) {
        CookieManager cookieManager = new CookieManager();
        cookieManager.setCookiePolicy(CookiePolicy.ACCEPT_ALL);

        HttpClient.Builder builder = HttpClient.newBuilder()
                .version(HttpClient.Version.HTTP_1_1)
                .connectTimeout(REQUEST_TIMEOUT)
                .cookieHandler(cookieManager);

        SSLContext sslContext = createSslContext(options);
        if (sslContext != null) {
            builder.sslContext(sslContext);
        }

        return builder.build();
    }

    private static X509TrustManager getDefaultTrustManager() throws Exception {
        TrustManagerFactory trustManagerFactory = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
        trustManagerFactory.init((KeyStore) null);
        return findX509TrustManager(trustManagerFactory.getTrustManagers());
    }

    private static X509TrustManager getCustomTrustManager(String certificateContent) throws Exception {
        CertificateFactory certificateFactory = CertificateFactory.getInstance("X.509");
        String normalizedPem = certificateContent.replace("\\n", "\n");
        KeyStore keyStore = KeyStore.getInstance(KeyStore.getDefaultType());
        keyStore.load(null, null);

        int certificateIndex = 0;
        for (var certificate : certificateFactory.generateCertificates(
                new ByteArrayInputStream(normalizedPem.getBytes(StandardCharsets.UTF_8)))) {
            keyStore.setCertificateEntry("beyondtrust-cert-" + certificateIndex++, certificate);
        }

        if (certificateIndex == 0) {
            throw new IllegalStateException("BEYONDTRUST_CERTIFICATE_CONTENT does not contain a valid certificate.");
        }

        TrustManagerFactory trustManagerFactory = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
        trustManagerFactory.init(keyStore);
        return findX509TrustManager(trustManagerFactory.getTrustManagers());
    }

    private static X509TrustManager findX509TrustManager(TrustManager[] trustManagers) {
        for (TrustManager trustManager : trustManagers) {
            if (trustManager instanceof X509TrustManager x509TrustManager) {
                return x509TrustManager;
            }
        }

        throw new IllegalStateException("X509TrustManager could not be created.");
    }

    private static SSLContext trustAllSslContext() throws Exception {
        SSLContext sslContext = SSLContext.getInstance("TLS");
        sslContext.init(null, new TrustManager[]{new X509TrustManager() {
            @Override
            public void checkClientTrusted(X509Certificate[] chain, String authType) {
            }

            @Override
            public void checkServerTrusted(X509Certificate[] chain, String authType) {
            }

            @Override
            public X509Certificate[] getAcceptedIssuers() {
                return new X509Certificate[0];
            }
        }}, new SecureRandom());

        return sslContext;
    }

    private static boolean hasValue(String value) {
        return value != null && !value.isBlank();
    }

    private static List<String> splitValues(String value, char... separators) {
        if (!hasValue(value)) {
            return Collections.emptyList();
        }

        List<Character> actualSeparators = new ArrayList<>();
        if (separators.length == 0) {
            actualSeparators.add(',');
            actualSeparators.add(';');
        } else {
            for (char separator : separators) {
                actualSeparators.add(separator);
            }
        }

        String normalized = value;
        for (char separator : actualSeparators) {
            normalized = normalized.replace(separator, ',');
        }

        List<String> values = new ArrayList<>();
        for (String item : normalized.split(",")) {
            String trimmed = item.trim();
            if (!trimmed.isEmpty()) {
                values.add(trimmed);
            }
        }

        return values;
    }

    private static String normalizeBaseUrl(String apiUrl) {
        if (!hasValue(apiUrl)) {
            throw new IllegalStateException("BEYONDTRUST_API_URL must be configured before creating the service.");
        }

        return apiUrl.replaceAll("/+$", "") + "/";
    }

    private static void ensureSuccess(int statusCode, String operationName) {
        if (!isSuccess(statusCode)) {
            throw new IllegalStateException(operationName + " failed with status " + statusCode + ".");
        }
    }

    private static boolean isSuccess(int statusCode) {
        return statusCode >= 200 && statusCode < 300;
    }

    private static String readValueIgnoreCase(JsonNode node, String propertyName) {
        if (node == null || !node.isObject()) {
            return null;
        }

        var fields = node.fields();
        while (fields.hasNext()) {
            var entry = fields.next();
            if (entry.getKey().equalsIgnoreCase(propertyName)) {
                return entry.getValue().asText();
            }
        }

        return null;
    }

    private static int readIntegerIgnoreCase(JsonNode node, String propertyName) {
        String value = readValueIgnoreCase(node, propertyName);
        if (value == null || value.isBlank()) {
            return -1;
        }

        try {
            return Integer.parseInt(value.trim());
        } catch (NumberFormatException ignored) {
            return -1;
        }
    }

    @Override
    public void close() {
    }

    private static final class CompositeX509TrustManager implements X509TrustManager {
        private final X509TrustManager defaultTrustManager;
        private final X509TrustManager customTrustManager;

        private CompositeX509TrustManager(X509TrustManager defaultTrustManager, X509TrustManager customTrustManager) {
            this.defaultTrustManager = defaultTrustManager;
            this.customTrustManager = customTrustManager;
        }

        @Override
        public void checkClientTrusted(X509Certificate[] chain, String authType) throws java.security.cert.CertificateException {
            try {
                defaultTrustManager.checkClientTrusted(chain, authType);
            } catch (java.security.cert.CertificateException ignored) {
                customTrustManager.checkClientTrusted(chain, authType);
            }
        }

        @Override
        public void checkServerTrusted(X509Certificate[] chain, String authType) throws java.security.cert.CertificateException {
            try {
                defaultTrustManager.checkServerTrusted(chain, authType);
            } catch (java.security.cert.CertificateException ignored) {
                customTrustManager.checkServerTrusted(chain, authType);
            }
        }

        @Override
        public X509Certificate[] getAcceptedIssuers() {
            X509Certificate[] defaultIssuers = defaultTrustManager.getAcceptedIssuers();
            X509Certificate[] customIssuers = customTrustManager.getAcceptedIssuers();
            X509Certificate[] acceptedIssuers = new X509Certificate[defaultIssuers.length + customIssuers.length];
            System.arraycopy(defaultIssuers, 0, acceptedIssuers, 0, defaultIssuers.length);
            System.arraycopy(customIssuers, 0, acceptedIssuers, defaultIssuers.length, customIssuers.length);
            return acceptedIssuers;
        }
    }
}
