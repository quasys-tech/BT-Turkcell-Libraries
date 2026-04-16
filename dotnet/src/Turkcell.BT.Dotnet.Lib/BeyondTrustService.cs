using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Turkcell.BT.Dotnet.Tests")]

namespace Turkcell.BT.Dotnet.Lib;

public sealed class BeyondTrustService : IDisposable
{
    private const int RequestTimeoutSeconds = 30;
    private readonly BeyondTrustOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public BeyondTrustService(BeyondTrustOptions options)
        : this(options, null)
    {
    }

    internal BeyondTrustService(BeyondTrustOptions options, HttpClient? httpClient)
    {
        _options = options;
        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? CreateHttpClient(options);
    }

    public async Task<Dictionary<string, string?>> FetchAllSecretsAsync()
    {
        var snapshot = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        await AuthenticateAsync().ConfigureAwait(false);

        if (_options.AllManagedAccountsEnabled || !string.IsNullOrWhiteSpace(_options.ManagedAccounts))
        {
            await ProcessManagedAccountsAsync(snapshot).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(_options.SecretSafePaths))
        {
            await ProcessSecretSafeAsync(snapshot).ConfigureAwait(false);
        }

        if (_options.AllSecretsEnabled)
        {
            Console.WriteLine("[BeyondTrust] BEYONDTRUST_ALL_SECRETS_ENABLED is accepted for compatibility, but Secret Safe loading still uses BEYONDTRUST_SECRET_SAFE_PATHS.");
        }

        return snapshot;
    }

    private async Task AuthenticateAsync()
    {
        if (_options.UseAppUser)
        {
            await LoginWithOAuthAsync().ConfigureAwait(false);
            return;
        }

        await LoginWithApiKeyAsync().ConfigureAwait(false);
    }

    private async Task LoginWithOAuthAsync()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "PS-Auth");

        var tokenResponse = await _httpClient.PostAsync(
                "Auth/Connect/Token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _options.ClientId ?? string.Empty,
                    ["client_secret"] = _options.ClientSecret ?? string.Empty
                }))
            .ConfigureAwait(false);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OAuth token request failed with status {(int)tokenResponse.StatusCode}.");
        }

        var tokenPayload = await tokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        var tokenDto = JsonSerializer.Deserialize<TokenResponseDto>(tokenPayload, _jsonOptions);
        var accessToken = tokenDto?.Access_Token;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("OAuth token response did not contain access_token.");
        }

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await PostSignAppInAsync().ConfigureAwait(false);
    }

    private async Task LoginWithApiKeyAsync()
    {
        if (!BeyondTrustAuthParsing.TryParseApiKey(_options.ApiKey, _options.RunAsUser, out var parsedApiKey))
        {
            throw new InvalidOperationException("Classic API authentication requires a valid BEYONDTRUST_API_KEY value.");
        }

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "Authorization",
            parsedApiKey!.ToAuthorizationHeader());

        await PostSignAppInAsync().ConfigureAwait(false);
    }

    private async Task PostSignAppInAsync()
    {
        var response = await _httpClient.PostAsync(
                "Auth/SignAppin",
                new StringContent("{}", Encoding.UTF8, "application/json"))
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Auth/SignAppin failed with status {(int)response.StatusCode}.");
        }
    }

    private async Task ProcessManagedAccountsAsync(IDictionary<string, string?> snapshot)
    {
        var response = await _httpClient.GetAsync("ManagedAccounts").ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"ManagedAccounts request failed with status {(int)response.StatusCode}.");
        }

        var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var allAccounts = JsonSerializer.Deserialize<List<ManagedAccountDto>>(payload, _jsonOptions) ?? [];
        var targetAccounts = FilterAccounts(allAccounts);

        foreach (var account in targetAccounts)
        {
            var password = await FetchPasswordWithRequestFlowAsync(account.SystemId, account.AccountId).ConfigureAwait(false);
            var configKey = $"bt.acc.{account.SystemName.Trim()}.{account.AccountName.Trim()}";
            snapshot[configKey] = password;
        }
    }

    private async Task ProcessSecretSafeAsync(IDictionary<string, string?> snapshot)
    {
        foreach (var path in SplitValues(_options.SecretSafePaths))
        {
            var response = await _httpClient.GetAsync($"Secrets-Safe/Secrets?Path={Uri.EscapeDataString(path)}").ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Secrets-Safe request for path '{path}' failed with status {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var items = JsonSerializer.Deserialize<List<SecretSafeItemDto>>(payload, _jsonOptions) ?? [];

            foreach (var item in items)
            {
                var folder = string.IsNullOrWhiteSpace(item.Folder) ? path : item.Folder.Trim();
                var title = string.IsNullOrWhiteSpace(item.Title) ? "Untitled" : item.Title.Trim();
                var baseKey = $"bt.safe.{folder}.{title}";

                snapshot[$"{baseKey}.password"] = item.Password ?? string.Empty;

                var userName = !string.IsNullOrWhiteSpace(item.Username)
                    ? item.Username.Trim()
                    : item.Account?.Trim();

                if (!string.IsNullOrWhiteSpace(userName))
                {
                    snapshot[$"{baseKey}.username"] = userName;
                }
            }
        }
    }

    private async Task<string> FetchPasswordWithRequestFlowAsync(int systemId, int accountId)
    {
        var requestId = string.Empty;

        try
        {
            var requestPayload = JsonSerializer.Serialize(new
            {
                systemId,
                accountId,
                durationMinutes = 5,
                reason = "TurkcellAutoFetch"
            });

            var createResponse = await _httpClient.PostAsync(
                    "Requests",
                    new StringContent(requestPayload, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            if (createResponse.IsSuccessStatusCode)
            {
                requestId = ParseRequestId(await createResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            else if (createResponse.StatusCode == HttpStatusCode.Conflict ||
                     createResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                requestId = await FindExistingRequestIdAsync(systemId, accountId).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"Request creation failed with status {(int)createResponse.StatusCode}.");
            }

            if (string.IsNullOrWhiteSpace(requestId))
            {
                throw new InvalidOperationException("Request ID could not be resolved for the managed account credential flow.");
            }

            for (var attempt = 0; attempt < 5; attempt++)
            {
                var credentialResponse = await _httpClient.GetAsync($"Credentials/{Uri.EscapeDataString(requestId)}").ConfigureAwait(false);
                if (credentialResponse.IsSuccessStatusCode)
                {
                    var credentialPayload = await credentialResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return ParseCredentialValue(credentialPayload);
                }

                await Task.Delay(TimeSpan.FromSeconds(attempt + 1)).ConfigureAwait(false);
            }

            throw new InvalidOperationException($"Credential retrieval failed for RequestID '{requestId}'.");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(requestId))
            {
                await TryCheckInAsync(requestId).ConfigureAwait(false);
            }
        }
    }

    private async Task TryCheckInAsync(string requestId)
    {
        try
        {
            var response = await _httpClient.PutAsync(
                    $"Requests/{Uri.EscapeDataString(requestId)}/Checkin",
                    new StringContent("{\"reason\":\"Done\"}", Encoding.UTF8, "application/json"))
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[BeyondTrust] Check-in failed for RequestID '{requestId}' with status {(int)response.StatusCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BeyondTrust] Check-in failed for RequestID '{requestId}': {ex.Message}");
        }
    }

    private async Task<string> FindExistingRequestIdAsync(int systemId, int accountId)
    {
        var response = await _httpClient.GetAsync("Requests").ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Requests lookup failed with status {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var item in document.RootElement.EnumerateArray())
        {
            if (TryGetInt32(item, "SystemID", out var currentSystemId) &&
                TryGetInt32(item, "AccountID", out var currentAccountId) &&
                currentSystemId == systemId &&
                currentAccountId == accountId &&
                TryGetPropertyIgnoreCase(item, "RequestID", out var requestIdElement))
            {
                return requestIdElement.ToString();
            }
        }

        return string.Empty;
    }

    internal static string ParseRequestId(string payload)
    {
        var trimmedPayload = payload.Trim();
        if (string.IsNullOrWhiteSpace(trimmedPayload))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmedPayload);
            var root = document.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.Number => root.ToString(),
                JsonValueKind.String => root.GetString() ?? string.Empty,
                JsonValueKind.Object when TryGetPropertyIgnoreCase(root, "RequestID", out var requestIdElement) => requestIdElement.ToString(),
                _ => string.Empty
            };
        }
        catch (JsonException)
        {
            return trimmedPayload.Trim('"');
        }
    }

    internal static string ParseCredentialValue(string payload)
    {
        var trimmedPayload = payload.Trim();
        if (string.IsNullOrWhiteSpace(trimmedPayload))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmedPayload);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString() ?? string.Empty;
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (TryGetPropertyIgnoreCase(root, "Credential", out var credential))
                {
                    return credential.ToString();
                }

                if (TryGetPropertyIgnoreCase(root, "Password", out var password))
                {
                    return password.ToString();
                }
            }
        }
        catch (JsonException)
        {
            return trimmedPayload.Trim('"');
        }

        return trimmedPayload.Trim('"');
    }

    private List<ManagedAccountDto> FilterAccounts(List<ManagedAccountDto> allAccounts)
    {
        if (_options.AllManagedAccountsEnabled)
        {
            return allAccounts;
        }

        var requestedAccounts = SplitValues(_options.ManagedAccounts, ';');
        if (requestedAccounts.Count == 0)
        {
            return [];
        }

        var requestedAccountSet = new HashSet<string>(requestedAccounts, StringComparer.OrdinalIgnoreCase);
        var filteredAccounts = allAccounts
            .Where(account => requestedAccountSet.Contains($"{account.SystemName.Trim()}.{account.AccountName.Trim()}"))
            .ToList();

        foreach (var missing in requestedAccountSet.Except(
                     filteredAccounts.Select(account => $"{account.SystemName.Trim()}.{account.AccountName.Trim()}"),
                     StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[BeyondTrust] Managed account was requested but not returned by the API: {missing}");
        }

        return filteredAccounts;
    }

    private static List<string> SplitValues(string? value, params char[] separators)
    {
        var actualSeparators = separators.Length == 0 ? [',', ';'] : separators;
        return string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(actualSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToList();
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool TryGetInt32(JsonElement element, string propertyName, out int value)
    {
        value = default;
        if (!TryGetPropertyIgnoreCase(element, propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetInt32(out value);
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return int.TryParse(property.GetString(), out value);
        }

        return false;
    }

    private static HttpClient CreateHttpClient(BeyondTrustOptions options)
    {
        var handler = CreateHandler(options);

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(NormalizeBaseUrl(options.ApiUrl)),
            Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds)
        };
    }

    private static HttpClientHandler CreateHandler(BeyondTrustOptions options)
    {
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        };

        if (options.IgnoreSslErrors)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return handler;
        }

        var trustedCertificates = LoadTrustedCertificates(options.CertificateContent);
        if (trustedCertificates.Count == 0)
        {
            return handler;
        }

        handler.ServerCertificateCustomValidationCallback = (_, certificate, _, sslPolicyErrors) =>
        {
            if (certificate is null)
            {
                return false;
            }

            var nonChainErrors = sslPolicyErrors & ~SslPolicyErrors.RemoteCertificateChainErrors;
            if (nonChainErrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            using var serverCertificate = certificate as X509Certificate2 ?? new X509Certificate2(certificate);

            if (trustedCertificates.Any(trusted =>
                    string.Equals(trusted.Thumbprint, serverCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            using var customChain = new X509Chain();
            customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            customChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            foreach (var trustedCertificate in trustedCertificates)
            {
                customChain.ChainPolicy.CustomTrustStore.Add(trustedCertificate);
                customChain.ChainPolicy.ExtraStore.Add(trustedCertificate);
            }

            if (!customChain.Build(serverCertificate))
            {
                return false;
            }

            var rootCertificate = customChain.ChainElements[^1].Certificate;
            return trustedCertificates.Any(trusted =>
                string.Equals(trusted.Thumbprint, rootCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
        };

        return handler;
    }

    private static List<X509Certificate2> LoadTrustedCertificates(string? certificateContent)
    {
        var certificates = new List<X509Certificate2>();
        if (string.IsNullOrWhiteSpace(certificateContent))
        {
            return certificates;
        }

        var normalizedPem = certificateContent.Replace("\\n", "\n", StringComparison.Ordinal);
        const string beginMarker = "-----BEGIN CERTIFICATE-----";
        const string endMarker = "-----END CERTIFICATE-----";

        var currentIndex = 0;
        while (true)
        {
            var beginIndex = normalizedPem.IndexOf(beginMarker, currentIndex, StringComparison.Ordinal);
            if (beginIndex < 0)
            {
                break;
            }

            var endIndex = normalizedPem.IndexOf(endMarker, beginIndex, StringComparison.Ordinal);
            if (endIndex < 0)
            {
                break;
            }

            endIndex += endMarker.Length;
            var singleCertificatePem = normalizedPem[beginIndex..endIndex];
            certificates.Add(X509Certificate2.CreateFromPem(singleCertificatePem));
            currentIndex = endIndex;
        }

        return certificates;
    }

    private static string NormalizeBaseUrl(string apiUrl)
    {
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            throw new InvalidOperationException("BEYONDTRUST_API_URL must be configured before creating the service.");
        }

        return $"{apiUrl.TrimEnd('/')}/";
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
