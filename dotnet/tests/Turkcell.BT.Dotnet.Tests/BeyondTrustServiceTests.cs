using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            IgnoreSslErrors = true,
            AllManagedAccountsEnabled = true
        };

        TestInfrastructure.ClearStaticCache();
    }

    [Fact]
    public async Task FetchAllSecretsAsync_WhenNoTargets_ShouldReturnEmpty()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
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
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) 
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"[{{\"SystemName\":\"{complexSystem}\",\"AccountName\":\"{account}\",\"SystemId\":101,\"AccountId\":202}}]", Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"REQ-999\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"TurkcellPassword123!\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"[{""SystemName"":""S1"",""AccountName"":""A1"",""SystemId"":1,""AccountId"":1}]", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"[{""SystemID"":1,""AccountID"":1,""RequestID"":555}]", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"ConflictPass\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

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
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(accountList, Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"REQ-1\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("\"Pass1\"", Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        using (var svc1 = new BeyondTrustService(_options))
        {
            TestInfrastructure.InjectMockClient(svc1, new HttpClient(handler1.Object) { BaseAddress = new Uri(_options.ApiUrl) });
            await svc1.FetchAllSecretsAsync();
        }

        var handler2 = new Mock<HttpMessageHandler>();
        handler2.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(accountList, Encoding.UTF8, "application/json") })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

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
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
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
    public void Configuration_AuthHeader_ShouldBeCorrect()
    {
        using var service = new BeyondTrustService(_options);
        var client = TestInfrastructure.GetPrivateField<HttpClient>(service, "_httpClient");
        var auth = client!.DefaultRequestHeaders.GetValues("Authorization").First();

        Assert.Contains("key=abc123", auth);
        Assert.Contains("runas=turkcell_user", auth);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}