using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

public class BeyondTrustServiceTests : IDisposable
{
    private readonly BeyondTrustOptions _options;

    public BeyondTrustServiceTests()
    {
        _options = new BeyondTrustOptions
        {
            Enabled = true,
            ApiUrl = "https://pam.test",
            ApiKey = "PS-Auth key=abc123; runas=turkcell_user;",
            // Testlerin mevcut mock kurgusu "API Key" akışına göre olduğu için 
            // AppUser modunu testlerde kapalı tutuyoruz.
            UseAppUser = false, 
            IgnoreSslErrors = true,
            AllManagedAccountsEnabled = true
        };

        TestInfrastructure.ClearStaticCache();
    }

    [Fact]
    public async Task FetchAllSecretsAsync_WhenNoTargets_ShouldReturnEmpty()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        
        // SignAppin için OK dönmeli (API Key modunda content okunmaz)
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using var service = new BeyondTrustService(_options);
        TestInfrastructure.InjectMockClient(service, new HttpClient(handlerMock.Object) { BaseAddress = new Uri(_options.ApiUrl) });

        _options.AllManagedAccountsEnabled = false;
        _options.ManagedAccounts = null;
        _options.SecretSafePaths = null;

        var result = await service.FetchAllSecretsAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task FetchAllSecretsAsync_FullFlow_Success_WithComplexNames()
    {
        var complexSystem = "dnsname (Db: PROD, Port:1521)";
        var account = "TURKCELL_ADMIN";
        var expectedKey = $"bt.acc.{complexSystem}.{account}";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // 1. SignAppin
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) // 2. ManagedAccounts List
            {
                Content = new StringContent($"[{{\"SystemName\":\"{complexSystem}\",\"AccountName\":\"{account}\",\"SystemId\":101,\"AccountId\":202}}]", Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"REQ-999\"", Encoding.UTF8, "application/json") }) // 3. Request
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"TurkcellPassword123!\"", Encoding.UTF8, "application/json") }) // 4. Credential
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // 5. Checkin

        using var service = new BeyondTrustService(_options);
        TestInfrastructure.InjectMockClient(service, new HttpClient(handlerMock.Object) { BaseAddress = new Uri(_options.ApiUrl) });

        var result = await service.FetchAllSecretsAsync();
        Assert.Equal("TurkcellPassword123!", result[expectedKey]);
    }

    [Fact]
    public async Task FetchAllSecretsAsync_WhenConflict_ShouldFindExistingRequest()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // 1. SignAppin
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"[{""SystemName"":""S1"",""AccountName"":""A1"",""SystemId"":1,""AccountId"":1}]", Encoding.UTF8, "application/json") }) // 2. List
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)) // 3. Request (Conflict)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"[{""SystemID"":1,""AccountID"":1,""RequestID"":555}]", Encoding.UTF8, "application/json") }) // 4. FindExisting (List Requests)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"ConflictPass\"", Encoding.UTF8, "application/json") }) // 5. Credential
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // 6. Checkin

        using var service = new BeyondTrustService(_options);
        TestInfrastructure.InjectMockClient(service, new HttpClient(handlerMock.Object) { BaseAddress = new Uri(_options.ApiUrl) });

        var result = await service.FetchAllSecretsAsync();
        Assert.Equal("ConflictPass", result["bt.acc.S1.A1"]);
    }

    [Fact]
    public async Task FetchAllSecretsAsync_CacheFallback_OnCriticalFail()
    {
        var accountList = @"[{""SystemName"":""S1"",""AccountName"":""A1"",""SystemId"":1,""AccountId"":1}]";
        var handler1 = new Mock<HttpMessageHandler>();
        handler1.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // SignAppin
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(accountList, Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"REQ-1\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"Pass1\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using (var svc1 = new BeyondTrustService(_options))
        {
            TestInfrastructure.InjectMockClient(svc1, new HttpClient(handler1.Object) { BaseAddress = new Uri(_options.ApiUrl) });
            await svc1.FetchAllSecretsAsync();
        }

        // İkinci servis çağrısı: Network hatası simülasyonu
        var handler2 = new Mock<HttpMessageHandler>();
        handler2.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // SignAppin (Success)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(accountList, Encoding.UTF8, "application/json") }) // List (Success)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError)); // Request (Fail) -> Cache'e gitmeli

        using (var svc2 = new BeyondTrustService(_options))
        {
            TestInfrastructure.InjectMockClient(svc2, new HttpClient(handler2.Object) { BaseAddress = new Uri(_options.ApiUrl) });
            var result = await svc2.FetchAllSecretsAsync();
            Assert.Equal("Pass1", result["bt.acc.S1.A1"]);
        }
    }

    [Fact]
    public async Task ProcessSecretSafeAsync_ComplexPath_Success()
    {
        _options.AllManagedAccountsEnabled = false;
        _options.ManagedAccounts = null;
        _options.SecretSafePaths = "TurkcellVault";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // SignAppin
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) // Secret Safe List
            {
                Content = new StringContent(@"[{""Folder"":""TurkcellVault"",""Title"":""DB_PASS"",""Username"":""sa"",""Password"":""secret123""}]", Encoding.UTF8, "application/json")
            });

        using var service = new BeyondTrustService(_options);
        TestInfrastructure.InjectMockClient(service, new HttpClient(handlerMock.Object) { BaseAddress = new Uri(_options.ApiUrl) });

        var result = await service.FetchAllSecretsAsync();
        var expectedKey = "bt.safe.TurkcellVault.DB_PASS.password";

        Assert.True(result.ContainsKey(expectedKey));
        Assert.Equal("secret123", result[expectedKey]);
    }

    [Fact]
    public async Task Configuration_AuthHeader_ShouldBeCorrect()
    {
        // GÜNCELLEME: Sadece header kontrolü yapacağımız için 
        // veri çekme işlemlerini kapatıyoruz. Yoksa mock setup'ı yetersiz kalıp Json hatası veriyor.
        _options.AllManagedAccountsEnabled = false;
        _options.ManagedAccounts = null;
        _options.SecretSafePaths = null;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // SignAppin cevabı

        using var service = new BeyondTrustService(_options);
        
        // Mock client enjekte et (yoksa gerçek adrese gitmeye çalışır)
        TestInfrastructure.InjectMockClient(service, new HttpClient(handlerMock.Object) { BaseAddress = new Uri(_options.ApiUrl) });

        // Header'ın set edilmesi için tetikle
        await service.FetchAllSecretsAsync();

        var client = TestInfrastructure.GetPrivateField<HttpClient>(service, "_httpClient");
        Assert.NotNull(client);

        var auth = client!.DefaultRequestHeaders.GetValues("Authorization").First();
        Assert.Contains("key=abc123", auth);
        Assert.Contains("runas=turkcell_user", auth);
    }

    [Fact]
    public void Ctor_WithPemBundle_ShouldConfigureCustomCertValidation_AndTrustKnownCert()
    {
        // Arrange: PEM path'e girelim
        _options.IgnoreSslErrors = false;

        // Bu cert'i bundle'a koyacağız ve callback'e "server cert" diye bunu vereceğiz.
        using var trustedCert = CreateSelfSignedCert("CN=trusted");

        var pemTrusted = ToPem(trustedCert);
        var pemOther = CreateSelfSignedPem("CN=other");

        _options.CertificateContent = pemTrusted + "\n" + pemOther;

        // Act
        using var service = new BeyondTrustService(_options);

        // Assert: handler callback set edilmiş olmalı
        var client = TestInfrastructure.GetPrivateField<HttpClient>(service, "_httpClient");
        Assert.NotNull(client);

        var handler = GetInnerHandler(client!);
        Assert.NotNull(handler);
        Assert.NotNull(handler!.ServerCertificateCustomValidationCallback);

        // Callback gerçekten "trustedCert" için true dönmeli (leaf match)
        var ok = handler.ServerCertificateCustomValidationCallback(
            new HttpRequestMessage(HttpMethod.Get, "https://pam.test/"),
            trustedCert,
            null,
            SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.True(ok);
    }

    private static HttpClientHandler? GetInnerHandler(HttpClient client)
    {
        // net8'de HttpClient -> HttpMessageInvoker._handler alanı var
        var fi = typeof(HttpMessageInvoker).GetField("_handler",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        return fi?.GetValue(client) as HttpClientHandler;
    }

    private static string CreateSelfSignedPem(string subjectName)
    {
        using var cert = CreateSelfSignedCert(subjectName);
        return ToPem(cert);
    }

    private static string ToPem(X509Certificate2 cert)
    {
        var der = cert.Export(X509ContentType.Cert);
        var b64 = Convert.ToBase64String(der);

        var sb = new StringBuilder();
        sb.AppendLine("-----BEGIN CERTIFICATE-----");
        for (int i = 0; i < b64.Length; i += 64)
            sb.AppendLine(b64.Substring(i, Math.Min(64, b64.Length - i)));
        sb.AppendLine("-----END CERTIFICATE-----");
        return sb.ToString();
    }

    private static X509Certificate2 CreateSelfSignedCert(string subjectName)
    {
        using var rsa = RSA.Create(2048);
        var req = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        return req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    public void Dispose() => GC.SuppressFinalize(this);
}