package com.turkcell.bt.java;

import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.DeserializationFeature;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.MapperFeature;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.json.JsonMapper;

import javax.net.ssl.*;
import java.net.*;
import java.net.http.*;
import java.nio.charset.StandardCharsets;
import java.security.SecureRandom;
import java.security.cert.X509Certificate;
import java.time.Duration;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

public class BeyondTrustService implements AutoCloseable {

    private final BeyondTrustOptions options;
    private final HttpClient httpClient;
    private final ObjectMapper objectMapper;
    private static final Map<String, String> passwordCache = new ConcurrentHashMap<>();

    public BeyondTrustService(BeyondTrustOptions options) {
        this.options = options;

        // 1. JSON Case Insensitive (ID=0 sorununu çözer)
        this.objectMapper = JsonMapper.builder()
                .configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true)
                .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false)
                .build();

        // 2. SSL & Hostname Bypass (Sertifika sorunlarını çözer)
        System.setProperty("jdk.internal.httpclient.disableHostnameVerification", "true");
        CookieManager cookieManager = new CookieManager();
        cookieManager.setCookiePolicy(CookiePolicy.ACCEPT_ALL);

        this.httpClient = HttpClient.newBuilder()
                .version(HttpClient.Version.HTTP_1_1)
                .connectTimeout(Duration.ofSeconds(30))
                .cookieHandler(cookieManager)
                .sslContext(trustAllSslContext())
                .build();
    }

    public BeyondTrustService(BeyondTrustOptions options, HttpClient httpClient) {
        this.options = options;
        this.objectMapper = JsonMapper.builder().configure(MapperFeature.ACCEPT_CASE_INSENSITIVE_PROPERTIES, true).configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false).build();
        this.httpClient = httpClient;
    }

    public Map<String, String> fetchAllSecrets() {
        Map<String, String> data = new HashMap<>();
        try {
            // SignAppin (Sessizce)
            try {
                httpClient.send(req("Auth/SignAppin").POST(HttpRequest.BodyPublishers.noBody()).header("Content-Type", "application/json").build(), HttpResponse.BodyHandlers.discarding());
            } catch (Exception ignored) {}

            // Managed Accounts
            if (options.isAllManagedAccountsEnabled() || (options.getManagedAccounts() != null && !options.getManagedAccounts().isBlank())) {
                processManagedAccounts(data);
            }
            // Secret Safe
            if (options.getSecretSafePaths() != null && !options.getSecretSafePaths().isBlank()) {
                processSecretSafe(data);
            }
        } catch (Exception ex) {
            System.err.println("❌ [BeyondTrust] fetchAllSecrets patladı:");
            ex.printStackTrace();        }
        return data;
    }

    private void processManagedAccounts(Map<String, String> dict) throws Exception {
        HttpResponse<String> resp = httpClient.send(req("ManagedAccounts").GET().build(), HttpResponse.BodyHandlers.ofString());
        if (resp.statusCode() != 200) return;

        List<ManagedAccountDto> accs = objectMapper.readValue(sanitize(resp.body()), new TypeReference<List<ManagedAccountDto>>() {});
        for (ManagedAccountDto acc : filterAccounts(accs)) {
            String key = "bt.acc." + acc.getSystemName().trim() + "." + acc.getAccountName().trim();
            dict.put(key, fetchPassword(acc.getSystemId(), acc.getAccountId(), "acc." + acc.getSystemName() + "." + acc.getAccountName()));
        }
    }

    private String fetchPassword(int sysId, int accId, String cacheKey) {
        String reqId = "";
        try {
            String body = String.format("{\"systemId\":%d,\"accountId\":%d,\"durationMinutes\":5,\"reason\":\"AutoFetch\"}", sysId, accId);
            HttpResponse<String> resp = httpClient.send(req("Requests").POST(HttpRequest.BodyPublishers.ofString(body)).header("Content-Type", "application/json").build(), HttpResponse.BodyHandlers.ofString());

            if (resp.statusCode() == 200 || resp.statusCode() == 201) reqId = parseId(sanitize(resp.body()));
            else if (resp.statusCode() == 409 || resp.statusCode() == 403) reqId = findExistingReqId(sysId, accId);

            if (reqId.isEmpty()) return passwordCache.getOrDefault(cacheKey, "ERROR_REQ_ID_NOT_FOUND");

            for (int i = 0; i < 3; i++) {
                HttpResponse<String> credResp = httpClient.send(req("Credentials/" + reqId).GET().build(), HttpResponse.BodyHandlers.ofString());
                if (credResp.statusCode() == 200) {
                    String pass = cleanPassword(credResp.body());
                    passwordCache.put(cacheKey, pass);
                    return pass;
                }
                Thread.sleep(1000);
            }
        } catch (Exception e) { return passwordCache.getOrDefault(cacheKey, "ERROR_EXCEPTION"); }
        finally {
            if (!reqId.isEmpty() && reqId.matches("\\d+")) {
                try { httpClient.send(req("Requests/" + reqId + "/Checkin").PUT(HttpRequest.BodyPublishers.ofString("{\"reason\":\"Done\"}")).header("Content-Type", "application/json").build(), HttpResponse.BodyHandlers.discarding()); } catch (Exception ignored) {}
            }
        }
        return passwordCache.getOrDefault(cacheKey, "ERROR_CRED_FAIL");
    }

    private void processSecretSafe(Map<String, String> dict) {
        if (options.getSecretSafePaths() == null) return;
        for (String path : options.getSecretSafePaths().split("[;,]")) {
            try {
                HttpResponse<String> resp = httpClient.send(req("Secrets-Safe/Secrets?Path=" + URLEncoder.encode(path.trim(), StandardCharsets.UTF_8)).GET().build(), HttpResponse.BodyHandlers.ofString());
                if (resp.statusCode() == 200) {
                    JsonNode root = objectMapper.readTree(sanitize(resp.body()));
                    if (root.isArray()) {
                        for (JsonNode item : root) {
                            String title = item.has("Title") ? item.get("Title").asText() : (item.has("title") ? item.get("title").asText() : "Untitled");
                            String folder = item.has("Folder") ? item.get("Folder").asText() : path.trim();
                            String baseKey = "bt.safe." + folder.trim() + "." + title.trim();
                            dict.put(baseKey + ".password", item.has("Password") ? item.get("Password").asText() : "");
                            if (item.has("Username")) dict.put(baseKey + ".username", item.get("Username").asText());
                        }
                    }
                }
            } catch (Exception ignored) {}
        }
    }

    // --- Helpers ---
    private String parseId(String b) { return b == null ? "" : b.replace("{", "").replace("}", "").replace("\"", "").replace("RequestID", "").replace("RequestId", "").replace(":", "").trim(); }

    private String findExistingReqId(int sysId, int accId) {
        try {
            HttpResponse<String> r = httpClient.send(req("Requests").GET().build(), HttpResponse.BodyHandlers.ofString());
            
            // Log ekleyelim: Requests listesi çekilebildi mi?
            if (r.statusCode() != 200) {
                System.err.println("⚠️ [BeyondTrust] Mevcut istekler listelenemedi. Status: " + r.statusCode());
                return "";
            }

            JsonNode root = objectMapper.readTree(sanitize(r.body()));
            if (root.isArray()) {
                for (JsonNode n : root) {
                    // Hem büyük hem küçük harf ihtimallerini güvenle kontrol ediyoruz
                    int s = -1;
                    if (n.has("SystemID")) s = n.get("SystemID").asInt();
                    else if (n.has("systemId")) s = n.get("systemId").asInt();

                    int a = -1;
                    if (n.has("AccountID")) a = n.get("AccountID").asInt();
                    else if (n.has("accountId")) a = n.get("accountId").asInt();

                    if (s == sysId && a == accId) {
                        String foundId = n.has("RequestID") ? n.get("RequestID").asText() : 
                                       (n.has("RequestId") ? n.get("RequestId").asText() : "");
                        System.out.println("ℹ️ [BeyondTrust] Mevcut bir RequestID bulundu: " + foundId);
                        return foundId;
                    }
                }
            }
        } catch (Exception e) {
            // "ignored" yerine artık buradayız!
            System.err.println("❌ [BeyondTrust] findExistingReqId içinde hata oluştu:");
            e.printStackTrace(); 
        }
        return "";
    }
    private HttpRequest.Builder req(String path) {
        String key = (options.getApiKey() != null ? options.getApiKey() : "").replace("PS-Auth", "").trim();
        String k = "", r = options.getRunAsUser() != null ? options.getRunAsUser() : "";
        for (String p : key.split(";")) {
            if (p.trim().toLowerCase().startsWith("key=")) k = p.trim().substring(4);
            else if (p.trim().toLowerCase().startsWith("runas=")) r = p.trim().substring(6);
            else if (k.isEmpty() && !p.trim().isEmpty()) k = p.trim();
        }
        String auth = "PS-Auth key=" + k + ";" + (!r.isEmpty() ? " runas=" + r + ";" : "");
        return HttpRequest.newBuilder().uri(URI.create(options.getApiUrl().replaceAll("/+$", "") + "/" + path)).version(HttpClient.Version.HTTP_1_1).header("Authorization", auth).timeout(Duration.ofSeconds(30));
    }

    private List<ManagedAccountDto> filterAccounts(List<ManagedAccountDto> all) {
        if (options.isAllManagedAccountsEnabled()) return all;
        if (options.getManagedAccounts() == null) return new ArrayList<>();
        Set<String> t = new HashSet<>();
        for (String s : options.getManagedAccounts().split(";")) { int i = s.lastIndexOf('.'); if (i > 0) t.add(s.substring(0, i).trim().toLowerCase() + "." + s.substring(i + 1).trim().toLowerCase()); }
        List<ManagedAccountDto> f = new ArrayList<>();
        for (ManagedAccountDto a : all) if (t.contains(a.getSystemName().trim().toLowerCase() + "." + a.getAccountName().trim().toLowerCase())) f.add(a);
        return f;
    }

    private String sanitize(String s) { return s == null ? "" : s.trim().replaceAll("^\"|\"$", "").replace("\\\"", "\""); }

    private String cleanPassword(String p) {
        p = sanitize(p);
        try { if (p.startsWith("{")) { JsonNode n = objectMapper.readTree(p); return n.has("Credential") ? n.get("Credential").asText() : (n.has("Password") ? n.get("Password").asText() : p); } } catch (Exception e) {}
        return p;
    }

    private SSLContext trustAllSslContext() {
        try {
            SSLContext ctx = SSLContext.getInstance("TLS");
            ctx.init(null, new TrustManager[]{new X509TrustManager() {
                public X509Certificate[] getAcceptedIssuers() { return null; }
                public void checkClientTrusted(X509Certificate[] c, String a) {}
                public void checkServerTrusted(X509Certificate[] c, String a) {}
            }}, new SecureRandom());
            return ctx;
        } catch (Exception e) { throw new RuntimeException(e); }
    }

    @Override public void close() {}
}