using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

public sealed class BeyondTrustServiceTests
{
    [Fact]
    public async Task FetchAllSecretsAsync_ClassicApiMode_UsesMergedAuthorizationHeader()
    {
        var requests = new List<HttpRequestMessage>();
        var service = CreateService(
            new BeyondTrustOptions
            {
                Enabled = true,
                ApiUrl = "https://pam.example.com/BeyondTrust/api/public/v3",
                UseAppUser = false,
                ApiKey = "raw-api-key",
                RunAsUser = "svc-demo"
            },
            async request =>
            {
                requests.Add(CloneRequest(request));
                await Task.Yield();
                return RouteHttpMessageHandler.Json(HttpStatusCode.OK, "[]");
            });

        var snapshot = await service.FetchAllSecretsAsync();

        Assert.Empty(snapshot);
        Assert.Equal("PS-Auth key=raw-api-key; runas=svc-demo;", requests.Single().Headers.Authorization?.ToString() ?? requests.Single().Headers.GetValues("Authorization").Single());
    }

    [Fact]
    public async Task FetchAllSecretsAsync_OAuthMode_UsesTokenAndBearerFlow()
    {
        var requests = new List<HttpRequestMessage>();
        using var service = CreateService(
            new BeyondTrustOptions
            {
                Enabled = true,
                ApiUrl = "https://pam.example.com/BeyondTrust/api/public/v3",
                UseAppUser = true,
                ClientId = "client-id",
                ClientSecret = "client-secret"
            },
            async request =>
            {
                requests.Add(CloneRequest(request));
                await Task.Yield();

                if (request.RequestUri!.AbsolutePath.EndsWith("/Auth/Connect/Token", StringComparison.Ordinal))
                {
                    return RouteHttpMessageHandler.Json(HttpStatusCode.OK, "{\"access_token\":\"oauth-token\"}");
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/Auth/SignAppin", StringComparison.Ordinal))
                {
                    return RouteHttpMessageHandler.Json(HttpStatusCode.OK, "{}");
                }

                return RouteHttpMessageHandler.Json(HttpStatusCode.OK, "[]");
            });

        await service.FetchAllSecretsAsync();

        var tokenRequest = requests.First(request => request.RequestUri!.AbsolutePath.EndsWith("/Auth/Connect/Token", StringComparison.Ordinal));
        var signInRequest = requests.First(request => request.RequestUri!.AbsolutePath.EndsWith("/Auth/SignAppin", StringComparison.Ordinal));

        Assert.Equal("PS-Auth", tokenRequest.Headers.GetValues("Authorization").Single());
        Assert.Equal("Bearer", signInRequest.Headers.Authorization?.Scheme);
        Assert.Equal("oauth-token", signInRequest.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task FetchAllSecretsAsync_ManagedAccounts_UsesExactKeyNamingAndConflictFlow()
    {
        using var service = CreateService(
            new BeyondTrustOptions
            {
                Enabled = true,
                ApiUrl = "https://pam.example.com/BeyondTrust/api/public/v3",
                UseAppUser = false,
                ApiKey = "PS-Auth key=api-key; runas=svc-demo;",
                ManagedAccounts = "System A.Account A"
            },
            request =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/Auth/SignAppin", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(HttpStatusCode.OK, "{}"));
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/ManagedAccounts", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(
                        HttpStatusCode.OK,
                        "[{\"SystemName\":\"System A\",\"AccountName\":\"Account A\",\"SystemID\":11,\"AccountID\":22}]"));
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/Requests", StringComparison.Ordinal) &&
                    request.Method == HttpMethod.Post)
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(HttpStatusCode.Conflict, "{}"));
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/Requests", StringComparison.Ordinal) &&
                    request.Method == HttpMethod.Get)
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(
                        HttpStatusCode.OK,
                        "[{\"RequestID\":444,\"SystemID\":11,\"AccountID\":22}]"));
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/Credentials/444", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(HttpStatusCode.OK, "\"managed-password\""));
                }

                if (request.RequestUri.AbsolutePath.EndsWith("/Requests/444/Checkin", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(HttpStatusCode.OK, "{}"));
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            });

        var snapshot = await service.FetchAllSecretsAsync();

        Assert.Equal("managed-password", snapshot["bt.acc.System A.Account A"]);
    }

    [Fact]
    public async Task FetchAllSecretsAsync_SecretSafe_UsesUsernameFallbackToAccount()
    {
        using var service = CreateService(
            new BeyondTrustOptions
            {
                Enabled = true,
                ApiUrl = "https://pam.example.com/BeyondTrust/api/public/v3",
                UseAppUser = false,
                ApiKey = "api-key",
                SecretSafePaths = "FolderA"
            },
            request =>
            {
                if (request.RequestUri!.AbsolutePath.EndsWith("/Auth/SignAppin", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(HttpStatusCode.OK, "{}"));
                }

                if (request.RequestUri.Query.Contains("Path=FolderA", StringComparison.Ordinal))
                {
                    return Task.FromResult(RouteHttpMessageHandler.Json(
                        HttpStatusCode.OK,
                        "[{\"Folder\":\"FolderA\",\"Title\":\"TitleA\",\"Account\":\"account-user\",\"Password\":\"secret-value\"}]"));
                }

                throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
            });

        var snapshot = await service.FetchAllSecretsAsync();

        Assert.Equal("secret-value", snapshot["bt.safe.FolderA.TitleA.password"]);
        Assert.Equal("account-user", snapshot["bt.safe.FolderA.TitleA.username"]);
    }

    [Theory]
    [InlineData("{\"RequestID\":123}", "123")]
    [InlineData("\"456\"", "456")]
    [InlineData("789", "789")]
    public void ParseRequestId_HandlesSupportedPayloadShapes(string payload, string expectedRequestId)
    {
        var requestId = BeyondTrustService.ParseRequestId(payload);

        Assert.Equal(expectedRequestId, requestId);
    }

    [Fact]
    public void Constructor_WithCertificateContent_UsesSecureCustomValidation()
    {
        using var certificate = CreateSelfSignedCertificate("CN=trusted.example.com");
        var options = new BeyondTrustOptions
        {
            ApiUrl = "https://trusted.example.com/BeyondTrust/api/public/v3",
            IgnoreSslErrors = false,
            CertificateContent = ToPem(certificate)
        };

        using var service = new BeyondTrustService(options);
        var handler = GetInnerHandler(TestInfrastructure.GetPrivateField<HttpClient>(service, "_httpClient")!);

        Assert.NotNull(handler);
        Assert.NotNull(handler!.ServerCertificateCustomValidationCallback);

        var isTrusted = handler.ServerCertificateCustomValidationCallback(
            new HttpRequestMessage(HttpMethod.Get, "https://trusted.example.com"),
            certificate,
            null,
            SslPolicyErrors.RemoteCertificateChainErrors);

        Assert.True(isTrusted);
    }

    private static BeyondTrustService CreateService(
        BeyondTrustOptions options,
        Func<HttpRequestMessage, Task<HttpResponseMessage>> router)
    {
        var httpClient = new HttpClient(new RouteHttpMessageHandler(router))
        {
            BaseAddress = new Uri($"{options.ApiUrl.TrimEnd('/')}/")
        };

        return new BeyondTrustService(options, httpClient);
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        clone.Content = request.Content;
        return clone;
    }

    private static HttpClientHandler? GetInnerHandler(HttpClient client)
    {
        var field = typeof(HttpMessageInvoker).GetField("_handler", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        return field?.GetValue(client) as HttpClientHandler;
    }

    private static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
    }

    private static string ToPem(X509Certificate2 certificate)
    {
        var base64 = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
        var builder = new StringBuilder();
        builder.AppendLine("-----BEGIN CERTIFICATE-----");
        for (var i = 0; i < base64.Length; i += 64)
        {
            builder.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
        }

        builder.AppendLine("-----END CERTIFICATE-----");
        return builder.ToString();
    }
}
