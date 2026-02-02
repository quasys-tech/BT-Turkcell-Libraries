using System.Text;
using System.Text.Json;
using System.Net;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

// Test projesinden internal modellere erişim sağlar
[assembly: InternalsVisibleTo("Turkcell.BT.Dotnet.Tests")]

namespace Turkcell.BT.Dotnet.Lib;

public class BeyondTrustService : IDisposable
{
    private readonly BeyondTrustOptions _options;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    // Fail-Safe Mekanizması: Uygulama ayakta kaldığı sürece son başarılı şifreleri korur.
    private static readonly ConcurrentDictionary<string, string> _passwordCache = new(StringComparer.OrdinalIgnoreCase);

    public BeyondTrustService(BeyondTrustOptions options)
    {
        _options = options;
        var baseUrl = string.IsNullOrWhiteSpace(_options.ApiUrl) ? "https://localhost/" : _options.ApiUrl.TrimEnd('/') + "/";

        var handler = new HttpClientHandler();

        if (_options.IgnoreSslErrors) 
        {
            // 1. Sertifika doğrulamasını tamamen bypass et
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            
            // 2. Bazı eski cache servisleri TLS 1.2 veya 1.1 bekleyebilir, kısıtlamayı kaldıralım
            handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
        }

        // Proxy ayarlarını devre dışı bırak (İç ağda hız kazanmak için)
        handler.UseProxy = false;
        handler.Proxy = null;

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(30) };

        ConfigureAuthentication();
    }

    private void ConfigureAuthentication()
    {
        var rawKey = (_options.ApiKey ?? "").Replace("PS-Auth", "", StringComparison.OrdinalIgnoreCase).Trim();
        string key = "", runas = _options.RunAsUser ?? "";

        foreach (var p in rawKey.Split(';'))
        {
            var part = p.Trim();
            if (part.StartsWith("key=", StringComparison.OrdinalIgnoreCase)) key = part[4..];
            else if (part.StartsWith("runas=", StringComparison.OrdinalIgnoreCase)) runas = part[6..];
            else if (string.IsNullOrEmpty(key)) key = part;
        }

        var authHeader = $"PS-Auth key={key};";
        if (!string.IsNullOrEmpty(runas)) authHeader += $" runas={runas};";
        
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
    }

    public async Task<Dictionary<string, string?>> FetchAllSecretsAsync()
    {
        var configData = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            // Session başlatma denemesi
            try { await _httpClient.PostAsync("Auth/SignAppin", new StringContent("{}", Encoding.UTF8, "application/json")).ConfigureAwait(false); } catch { }

            if (_options.AllManagedAccountsEnabled || !string.IsNullOrWhiteSpace(_options.ManagedAccounts))
            {
                await ProcessManagedAccountsAsync(configData).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(_options.SecretSafePaths))
            {
                await ProcessSecretSafeAsync(configData).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ [Turkcell.BT.BeyondTrust] Service Error: {ex.Message}");
            // Turkcell standartlarında hata yutulmaz, provider katmanında loglanması için yukarı fırlatılır.
            throw; 
        }

        return configData;
    }

    private async Task ProcessManagedAccountsAsync(Dictionary<string, string?> dict)
    {
        var listResp = await _httpClient.GetAsync("ManagedAccounts").ConfigureAwait(false);
        if (!listResp.IsSuccessStatusCode) return;

        var listJson = await listResp.Content.ReadAsStringAsync().ConfigureAwait(false);
        var allAccounts = JsonSerializer.Deserialize<List<ManagedAccountDto>>(listJson, _jsonOptions) ?? [];
        
        var targets = FilterAccounts(allAccounts);

        // .NET 8 paralel işleme - Turkcell iç ağında eşzamanlı 5 istek idealdir.
        await Parallel.ForEachAsync(targets, new ParallelOptions { MaxDegreeOfParallelism = 5 }, async (acc, _) =>
        {
            string cacheKey = $"acc.{acc.SystemName.Trim()}.{acc.AccountName.Trim()}";
            string password = await FetchPasswordWithRequestFlowAsync(acc.SystemId, acc.AccountId, cacheKey).ConfigureAwait(false);
            
            string configKey = $"bt.acc.{acc.SystemName.Trim()}.{acc.AccountName.Trim()}";
            lock (dict) { dict[configKey] = password; }
        }).ConfigureAwait(false);
    }

    private async Task ProcessSecretSafeAsync(Dictionary<string, string?> dict)
    {
        var paths = _options.SecretSafePaths!.Split([';', ','], StringSplitOptions.RemoveEmptyEntries);
        foreach (var path in paths)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"Secrets-Safe/Secrets?Path={Uri.EscapeDataString(path.Trim())}").ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) continue;

                var json = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var items = JsonSerializer.Deserialize<List<SecretSafeItemDto>>(json, _jsonOptions);
                
                if (items == null) continue;

                foreach (var item in items)
                {
                    var folder = item.Folder ?? path;
                    var title = item.Title ?? "Untitled";
                    var baseKey = $"bt.safe.{folder.Trim()}.{title.Trim()}";
                    
                    lock (dict)
                    {
                        string secretVal = item.Password ?? "";
                        dict[$"{baseKey}.password"] = secretVal;

                        if (!string.IsNullOrWhiteSpace(item.Username) || !string.IsNullOrWhiteSpace(item.Account))
                        {
                            dict[$"{baseKey}.username"] = item.Username ?? item.Account;
                        }

                        _passwordCache[$"safe.{folder.Trim()}.{title.Trim()}"] = secretVal;
                    }
                }
            }
            catch { /* Gizli kasa hataları genel akışı bozmamalı */ }
        }
    }

    private async Task<string> FetchPasswordWithRequestFlowAsync(int sysId, int accId, string cacheKey)
    {
        string reqId = "";
        try
        {
            // Raw string literal (.NET 8/C# 12 style)
            var jsonBody = $$"""{"systemId":{{sysId}},"accountId":{{accId}},"durationMinutes":5,"reason":"TurkcellAutoFetch"}""";
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            var reqResp = await _httpClient.PostAsync("Requests", content).ConfigureAwait(false);
            
            if (reqResp.IsSuccessStatusCode)
            {
                reqId = ParseRequestId(await reqResp.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            else if (reqResp.StatusCode == HttpStatusCode.Conflict) 
            {
                reqId = await FindExistingRequestIdAsync(sysId, accId).ConfigureAwait(false);
            }

            if (string.IsNullOrWhiteSpace(reqId)) return GetFromCacheOrError(cacheKey, "REQ_ID_NOT_FOUND");

            string pass = "";
            bool success = false;

            // Turkcell iç ağında 5 deneme (Exponential backoff simülasyonu)
            for (int i = 0; i < 5; i++)
            {
                var credResp = await _httpClient.GetAsync($"Credentials/{reqId}").ConfigureAwait(false);
                if (credResp.IsSuccessStatusCode)
                {
                    pass = (await credResp.Content.ReadAsStringAsync().ConfigureAwait(false)).Trim();
                    success = true;
                    break; 
                }
                await Task.Delay(1000 * (i + 1)).ConfigureAwait(false);
            }

            if (!success) return GetFromCacheOrError(cacheKey, "CRED_FAIL");

            pass = CleanPassword(pass);
            _passwordCache[cacheKey] = pass;
            return pass;
        }
        catch { return GetFromCacheOrError(cacheKey, "EXCEPTION"); }
        finally
        {
            if (!string.IsNullOrEmpty(reqId) && reqId.All(char.IsDigit))
            {
                _ = _httpClient.PutAsync($"Requests/{reqId}/Checkin", new StringContent("""{"reason":"Done"}""", Encoding.UTF8, "application/json")).ConfigureAwait(false);
            }
        }
    }

    private string CleanPassword(string pass)
    {
        if (pass.StartsWith('\"') && pass.EndsWith('\"')) pass = pass[1..^1];
        if (!pass.StartsWith('{')) return pass;

        using var doc = JsonDocument.Parse(pass);
        if (doc.RootElement.TryGetProperty("Credential", out var v)) return v.ToString();
        if (doc.RootElement.TryGetProperty("Password", out v)) return v.ToString();
        return pass;
    }

    private string GetFromCacheOrError(string key, string errorDetail) => 
        _passwordCache.TryGetValue(key, out var cachedValue) ? cachedValue : $"ERROR_{errorDetail}";

    private async Task<string> FindExistingRequestIdAsync(int sysId, int accId)
    {
        try
        {
            var resp = await _httpClient.GetAsync("Requests").ConfigureAwait(false); 
            if (!resp.IsSuccessStatusCode) 
            {
                Console.WriteLine($"⚠️ [BeyondTrust] Requests listelenemedi. Status: {resp.StatusCode}");
                return "";
            }

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return "";

            foreach (var req in doc.RootElement.EnumerateArray())
            {
                // Case-insensitive mülk okuma
                int s = GetIntProp(req, "SystemID");
                int a = GetIntProp(req, "AccountID");

                if (s == sysId && a == accId)
                {
                    // RequestID veya RequestId her ikisini de kontrol et
                    var ridProp = req.EnumerateObject()
                        .FirstOrDefault(p => p.Name.Equals("RequestID", StringComparison.OrdinalIgnoreCase));
                    
                    return ridProp.Value.ValueKind != JsonValueKind.Undefined ? ridProp.Value.ToString() : "";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [BeyondTrust] findExistingReqId Hatası: {ex.Message}");
        }
        return "";
    }

    private int GetIntProp(JsonElement el, string name)
    {
        var prop = el.EnumerateObject().FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return prop.Value.ValueKind == JsonValueKind.Number ? prop.Value.GetInt32() : -1;
    }

    private string ParseRequestId(string body) => 
        body.Replace("{", "").Replace("}", "").Replace("\"", "").Replace("RequestID", "").Replace(":", "").Trim();

    private List<ManagedAccountDto> FilterAccounts(List<ManagedAccountDto> all)
    {
        if (_options.AllManagedAccountsEnabled) return all;
        if (string.IsNullOrWhiteSpace(_options.ManagedAccounts)) return [];

        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in _options.ManagedAccounts.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = s.LastIndexOf('.'); 
            if (idx > 0) 
            {
                targets.Add($"{s[..idx].Trim()}.{s[(idx + 1)..].Trim()}");
            }
        }
        return all.Where(a => targets.Contains($"{a.SystemName.Trim()}.{a.AccountName.Trim()}")).ToList();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}