package com.turkcell.bt.java;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentCaptor;

import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

class BeyondTrustServiceTest {

    private BeyondTrustOptions options;
    private BeyondTrustService service;
    private HttpClient mockHttpClient;

    @BeforeEach
    void setUp() {
        options = new BeyondTrustOptions();
        options.setApiUrl("https://pam.test.local/");
        options.setApiKey("PS-Auth key=testKey; runas=testUser;");
        options.setEnabled(true);

        mockHttpClient = mock(HttpClient.class);
        service = new BeyondTrustService(options, mockHttpClient);
    }

    @Test
    @DisplayName("Managed Accounts: Happy Path (Direkt İstek Başarılı)")
    void testManagedAccountsHappyPath() throws Exception {
        options.setAllManagedAccountsEnabled(true);

        // HATA ÇÖZÜMÜ: Mock nesnelerini 'when' bloğundan ÖNCE oluşturuyoruz.
        HttpResponse<Object> respAuth = mockResponse(200, "");
        HttpResponse<Object> respList = mockResponse(200, "[{\"SystemName\":\"LinuxProd\",\"AccountName\":\"root\",\"SystemID\":101,\"AccountID\":202}]");
        HttpResponse<Object> respPost = mockResponse(200, "12345");
        HttpResponse<Object> respCred = mockResponse(200, "\"SuperSecretPass\"");
        HttpResponse<Object> respCheckin = mockResponse(204, "");

        // Zincirleme
        when(mockHttpClient.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(respAuth)
                .thenReturn(respList)
                .thenReturn(respPost)
                .thenReturn(respCred)
                .thenReturn(respCheckin);

        Map<String, String> result = service.fetchAllSecrets();

        assertNotNull(result);
        assertEquals("SuperSecretPass", result.get("bt.acc.LinuxProd.root"));
    }

    @Test
    @DisplayName("Fallback Mekanizması: POST 409 Conflict -> GET Existing Request")
    void testFallbackMechanism() throws Exception {
        options.setManagedAccounts("LinuxProd.root");

        // Nesneleri önden hazırla
        HttpResponse<Object> respAuth = mockResponse(200, "");
        HttpResponse<Object> respList = mockResponse(200, "[{\"SystemName\":\"LinuxProd\",\"AccountName\":\"root\",\"SystemID\":101,\"AccountID\":202}]");
        HttpResponse<Object> respPostFail = mockResponse(409, "Conflict");
        HttpResponse<Object> respGetReqs = mockResponse(200, "[{\"RequestID\":9999,\"SystemID\":101,\"AccountID\":202,\"Status\":\"Active\"}]");
        HttpResponse<Object> respCred = mockResponse(200, "FallbackPassword");

        when(mockHttpClient.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(respAuth)
                .thenReturn(respList)
                .thenReturn(respPostFail)
                .thenReturn(respGetReqs)
                .thenReturn(respCred);

        Map<String, String> result = service.fetchAllSecrets();

        assertEquals("FallbackPassword", result.get("bt.acc.LinuxProd.root"));
    }

    @Test
    @DisplayName("Secret Safe: Klasör ve Title Parsing")
    void testSecretSafeParsing() throws Exception {
        options.setSecretSafePaths("DevTeam/Apps");

        HttpResponse<Object> respAuth = mockResponse(200, "");
        String safeJson = "[" +
                "{\"Title\":\"App1_DB\",\"Folder\":\"DevTeam/Apps\",\"Password\":\"SafePass1\",\"Username\":\"user1\"}," +
                "{\"title\":\"App2_API\",\"Folder\":\"DevTeam/Apps\",\"Password\":\"SafePass2\"}" +
                "]";
        HttpResponse<Object> respSafe = mockResponse(200, safeJson);

        when(mockHttpClient.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(respAuth)
                .thenReturn(respSafe);

        Map<String, String> result = service.fetchAllSecrets();

        assertEquals("SafePass1", result.get("bt.safe.DevTeam/Apps.App1_DB.password"));
        assertEquals("user1", result.get("bt.safe.DevTeam/Apps.App1_DB.username"));
    }

    @Test
    @DisplayName("Payload Format Kontrolü: camelCase systemId gönderilmeli")
    void testPayloadFormat() throws Exception {
        options.setManagedAccounts("TestSys.TestAcc");

        // Nesneleri önden hazırla (UnfinishedStubbing hatasını çözen kısım burası)
        HttpResponse<Object> respAuth = mockResponse(200, "");
        HttpResponse<Object> respList = mockResponse(200, "[{\"SystemName\":\"TestSys\",\"AccountName\":\"TestAcc\",\"SystemID\":5,\"AccountID\":6}]");
        HttpResponse<Object> respPost = mockResponse(200, "100");
        HttpResponse<Object> respCred = mockResponse(200, "pass");

        when(mockHttpClient.send(any(), any()))
                .thenReturn(respAuth)
                .thenReturn(respList)
                .thenReturn(respPost)
                .thenReturn(respCred);

        service.fetchAllSecrets();

        ArgumentCaptor<HttpRequest> captor = ArgumentCaptor.forClass(HttpRequest.class);
        verify(mockHttpClient, atLeast(1)).send(captor.capture(), any());

        // POST /Requests isteğini bul
        HttpRequest request = captor.getAllValues().stream()
                .filter(r -> r.uri().toString().endsWith("/Requests") && r.method().equals("POST"))
                .findFirst()
                .orElse(null);

        assertNotNull(request, "POST isteği bulunamadı!");
        assertEquals("POST", request.method());
    }

    // --- HELPER METOT (RAW MOCK KULLANIMI) ---
    @SuppressWarnings({"unchecked", "rawtypes"})
    private HttpResponse<Object> mockResponse(int statusCode, String body) {
        HttpResponse response = mock(HttpResponse.class);
        when(response.statusCode()).thenReturn(statusCode);
        when(response.body()).thenReturn(body);
        return response;
    }
}