package com.turkcell.bt.java;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentCaptor;

import javax.net.ssl.SSLContext;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

class BeyondTrustServiceTest {

    private static final String TEST_PEM = """
            -----BEGIN CERTIFICATE-----
            MIIC6jCCAdKgAwIBAgIJAIxowur/x1WsMA0GCSqGSIb3DQEBCwUAMBcxFTATBgNV
            BAMTDGJ0LWphdmEtdGVzdDAeFw0yNjA0MTUxNTU4NDdaFw0yNjA1MTYxNTU4NDda
            MBcxFTATBgNVBAMTDGJ0LWphdmEtdGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEP
            ADCCAQoCggEBAJpDZVqd0TwQekEmbiNsYkcgR9xPnVD9NWYmmvEPpZFQWnoFGP/H
            kPw9/GUXxvuhev+jgYn/34AqTvmfQTvmjxCJzXZnWnhrQg02JNG9RHB6r26s6S2Q
            SU3RxGhIM+Ml89HDwPVHy9v+fyeACNwkk6n7ZifINFpeQKADQM64htChpMo2+JYh
            uiYTKe8qdMtD3935ZYVehK1mA8nVaBXKtiizx9MqxC/FbRSI2TALTlQzBw1jf6gi
            wjHzEAv/qznLb9M6j7mPT+0/Zp7MoVD3Dj6iubWYmdzG/YWSCOT7OnlKsBShDmIF
            mslyrpA5Pf7Vb/WhIKzSi7V6tAaKxngvU7kCAwEAAaM5MDcwCQYDVR0TBAIwADAL
            BgNVHQ8EBAMCB4AwHQYDVR0OBBYEFLqZsO9eeYrqCLSAV4GL2PwiuO3sMA0GCSqG
            SIb3DQEBCwUAA4IBAQAHWH6Y454QU3mX2Zex8m1pcXJRARv2si14Va3rpt2lBgdq
            uMF97G9mOpkWenzMO/y+5sC9IhGqRBLAR8KBcwgr+4kxjuIjg7TDGz5QUrMhHisk
            tgX6+6ts2eRjXebz56ViTbJ1FX/w80/1MX/QXiTwePnOSuypM2c3O0TYZNyMgJhC
            /dpTlEVOcjlXitCnxTHeUBZPcCPo79SbD8b0ddspX4oGhQyuyu0QCXf98Wg1HIB3
            mZdMtO0kfF8+YXA5yncRYxwyDP55/rdOnGLjjTkBYRPxLluBpKXcrBwsvQaEoAfy
            lDBe4+VlaVe9XkBAbZCTJmZk3CteUPc7RHvwnRDz
            -----END CERTIFICATE-----
            """;

    @Test
    @DisplayName("Classic API mode saf api-key ve runas degerini PS-Auth header olarak birlestirmeli")
    void classicApiModeBuildsMergedAuthHeader() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);
        options.setApiKey("raw-api-key");
        options.setRunAsUser("svc-demo");
        options.setSecretSafePaths("FolderA");

        HttpClient client = mock(HttpClient.class);
        HttpResponse<String> signInResponse = mockResponse(200, "{}");
        HttpResponse<String> secretSafeResponse = mockResponse(200, "[{\"Folder\":\"FolderA\",\"Title\":\"TitleA\",\"Account\":\"account-user\",\"Password\":\"secret-value\"}]");
        when(client.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(signInResponse)
                .thenReturn(secretSafeResponse);

        BeyondTrustService service = new BeyondTrustService(options, client);
        Map<String, String> snapshot = service.fetchAllSecrets();

        assertEquals("secret-value", snapshot.get("bt.safe.FolderA.TitleA.password"));
        assertEquals("account-user", snapshot.get("bt.safe.FolderA.TitleA.username"));

        ArgumentCaptor<HttpRequest> requestCaptor = ArgumentCaptor.forClass(HttpRequest.class);
        verify(client, atLeastOnce()).send(requestCaptor.capture(), any());

        HttpRequest signInRequest = requestCaptor.getAllValues().stream()
                .filter(request -> request.uri().toString().endsWith("/Auth/SignAppin"))
                .findFirst()
                .orElseThrow();

        assertEquals("PS-Auth key=raw-api-key; runas=svc-demo;",
                signInRequest.headers().firstValue("Authorization").orElseThrow());
    }

    @Test
    @DisplayName("OAuth mode token alip sonraki isteklerde Bearer header kullanmali")
    void oauthModeUsesBearerAfterToken() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(true);
        options.setClientId("client-id");
        options.setClientSecret("client-secret");
        options.setSecretSafePaths("FolderA");

        HttpClient client = mock(HttpClient.class);
        HttpResponse<String> tokenResponse = mockResponse(200, "{\"access_token\":\"oauth-token\"}");
        HttpResponse<String> signInResponse = mockResponse(200, "{}");
        HttpResponse<String> secretSafeResponse = mockResponse(200, "[]");
        when(client.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(tokenResponse)
                .thenReturn(signInResponse)
                .thenReturn(secretSafeResponse);

        BeyondTrustService service = new BeyondTrustService(options, client);
        service.fetchAllSecrets();

        ArgumentCaptor<HttpRequest> requestCaptor = ArgumentCaptor.forClass(HttpRequest.class);
        verify(client, atLeastOnce()).send(requestCaptor.capture(), any());

        HttpRequest tokenRequest = requestCaptor.getAllValues().stream()
                .filter(request -> request.uri().toString().endsWith("/Auth/Connect/Token"))
                .findFirst()
                .orElseThrow();

        HttpRequest signInRequest = requestCaptor.getAllValues().stream()
                .filter(request -> request.uri().toString().endsWith("/Auth/SignAppin"))
                .findFirst()
                .orElseThrow();

        assertEquals("PS-Auth", tokenRequest.headers().firstValue("Authorization").orElseThrow());
        assertEquals("Bearer oauth-token", signInRequest.headers().firstValue("Authorization").orElseThrow());
    }

    @Test
    @DisplayName("Managed account conflict flow exact key naming ile credential cekebilmeli")
    void managedAccountConflictFlowUsesExactKeyNaming() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(true);
        options.setApiUrl("https://pam.example.com/BeyondTrust/api/public/v3");
        options.setUseAppUser(false);
        options.setApiKey("api-key");
        options.setManagedAccounts("System A.Account A");

        HttpClient client = mock(HttpClient.class);
        HttpResponse<String> signInResponse = mockResponse(200, "{}");
        HttpResponse<String> managedAccountsResponse = mockResponse(200, "[{\"SystemName\":\"System A\",\"AccountName\":\"Account A\",\"SystemID\":11,\"AccountID\":22}]");
        HttpResponse<String> createRequestResponse = mockResponse(409, "{}");
        HttpResponse<String> existingRequestResponse = mockResponse(200, "[{\"RequestID\":444,\"SystemID\":11,\"AccountID\":22}]");
        HttpResponse<String> credentialResponse = mockResponse(200, "\"managed-password\"");
        HttpResponse<String> checkInResponse = mockResponse(200, "{}");
        when(client.send(any(HttpRequest.class), any(HttpResponse.BodyHandler.class)))
                .thenReturn(signInResponse)
                .thenReturn(managedAccountsResponse)
                .thenReturn(createRequestResponse)
                .thenReturn(existingRequestResponse)
                .thenReturn(credentialResponse)
                .thenReturn(checkInResponse);

        BeyondTrustService service = new BeyondTrustService(options, client);
        Map<String, String> snapshot = service.fetchAllSecrets();

        assertEquals("managed-password", snapshot.get("bt.acc.System A.Account A"));
    }

    @Test
    @DisplayName("parseRequestId object string ve numeric payload formatlarini desteklemeli")
    void parseRequestIdSupportsMultiplePayloadShapes() {
        assertEquals("123", BeyondTrustService.parseRequestId("{\"RequestID\":123}"));
        assertEquals("456", BeyondTrustService.parseRequestId("\"456\""));
        assertEquals("789", BeyondTrustService.parseRequestId("789"));
    }

    @Test
    @DisplayName("Certificate content verildiginde custom SSL context olusturulmali")
    void createSslContextUsesCustomCertificateContent() throws Exception {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setCertificateContent(TEST_PEM);

        SSLContext sslContext = BeyondTrustService.createSslContext(options);

        assertNotNull(sslContext);
        assertNotSame(SSLContext.getDefault(), sslContext);
    }

    @Test
    @DisplayName("Ignore SSL hatalari sadece acik opt-in oldugunda trust-all context dondurmeli")
    void createSslContextSupportsIgnoreSslErrorsOptIn() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setIgnoreSslErrors(true);

        assertNotNull(BeyondTrustService.createSslContext(options));
    }

    @SuppressWarnings({"unchecked", "rawtypes"})
    private HttpResponse<String> mockResponse(int statusCode, String body) {
        HttpResponse response = mock(HttpResponse.class);
        when(response.statusCode()).thenReturn(statusCode);
        when(response.body()).thenReturn(body);
        return response;
    }
}
