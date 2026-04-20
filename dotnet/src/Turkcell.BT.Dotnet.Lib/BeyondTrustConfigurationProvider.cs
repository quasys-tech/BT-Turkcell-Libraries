using Microsoft.Extensions.Configuration;

namespace Turkcell.BT.Dotnet.Lib;

public sealed class BeyondTrustConfigurationSource : IConfigurationSource
{
    private readonly BeyondTrustOptions _options;

    public BeyondTrustConfigurationSource(BeyondTrustOptions options)
    {
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new BeyondTrustConfigurationProvider(_options);
    }
}

public class BeyondTrustConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly BeyondTrustOptions _options;
    private readonly Func<Task<Dictionary<string, string?>>> _snapshotLoader;
    private readonly object _syncRoot = new();
    private readonly bool _debugEnabled = string.Equals(
        Environment.GetEnvironmentVariable("BEYONDTRUST_DEBUG"),
        "true",
        StringComparison.OrdinalIgnoreCase);
    private Timer? _refreshTimer;

    public BeyondTrustConfigurationProvider(BeyondTrustOptions options)
        : this(
            options,
            async () =>
            {
                using var service = new BeyondTrustService(options);
                return await service.FetchAllSecretsAsync().ConfigureAwait(false);
            })
    {
    }

    internal BeyondTrustConfigurationProvider(BeyondTrustOptions options, Func<Task<Dictionary<string, string?>>> snapshotLoader)
    {
        _options = options;
        _snapshotLoader = snapshotLoader;
    }

    public override void Load()
    {
        if (!_options.Enabled)
        {
            Console.WriteLine("[BeyondTrust] Initial load skipped because the provider is disabled.");
            return;
        }

        if (_debugEnabled)
        {
            Console.WriteLine(
                $"[BeyondTrust][DEBUG] Provider load started. Auth mode: {(_options.UseAppUser ? "OAuth App User" : "Classic API")}, " +
                $"Refresh interval: {_options.RefreshIntervalSeconds}s, Managed accounts: {_options.ManagedAccounts ?? "<empty>"}, " +
                $"All managed accounts enabled: {_options.AllManagedAccountsEnabled}, Secret Safe paths: {_options.SecretSafePaths ?? "<empty>"}");
        }

        lock (_syncRoot)
        {
            if (TryLoadSnapshot("Initial load", out var snapshot))
            {
                Data = snapshot;
                Console.WriteLine($"[BeyondTrust] Initial load completed. Loaded {snapshot.Count} key(s).");
            }
            else
            {
                Console.WriteLine("[BeyondTrust] Initial load failed. Keeping empty configuration snapshot.");
            }

            if (_options.RefreshIntervalSeconds > 0)
            {
                var interval = TimeSpan.FromSeconds(_options.RefreshIntervalSeconds);
                _refreshTimer?.Dispose();
                _refreshTimer = new Timer(DoRefresh, null, interval, interval);
                Console.WriteLine($"[BeyondTrust] Background refresh enabled with {_options.RefreshIntervalSeconds}s interval.");
            }
            else
            {
                Console.WriteLine("[BeyondTrust] Background refresh is disabled because BEYONDTRUST_REFRESH_INTERVAL=0.");
            }
        }
    }

    private void DoRefresh(object? _)
    {
        lock (_syncRoot)
        {
            if (_debugEnabled)
            {
                Console.WriteLine("[BeyondTrust][DEBUG] Refresh cycle started.");
            }

            if (!TryLoadSnapshot("Refresh", out var snapshot))
            {
                Console.WriteLine("[BeyondTrust] Refresh failed. Keeping the last successful snapshot.");
                return;
            }

            if (!DictionaryEquals(
                    new Dictionary<string, string?>(Data, StringComparer.OrdinalIgnoreCase),
                    snapshot))
            {
                Data = snapshot;
                OnReload();

                if (_debugEnabled)
                {
                    Console.WriteLine($"[BeyondTrust][DEBUG] Refresh applied. Loaded {snapshot.Count} key(s).");
                }
            }
            else if (_debugEnabled)
            {
                Console.WriteLine("[BeyondTrust][DEBUG] Refresh completed with no snapshot changes.");
            }
        }
    }

    private bool TryLoadSnapshot(string operation, out Dictionary<string, string?> snapshot)
    {
        snapshot = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var loadedSnapshot = _snapshotLoader().GetAwaiter().GetResult() ??
                                 new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            snapshot = new Dictionary<string, string?>(loadedSnapshot, StringComparer.OrdinalIgnoreCase);

            if (_debugEnabled)
            {
                Console.WriteLine($"[BeyondTrust][DEBUG] {operation} snapshot load succeeded with {snapshot.Count} key(s).");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BeyondTrust] {operation} failed: {ex.Message}");
            return false;
        }
    }

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, string?> left,
        IReadOnlyDictionary<string, string?> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var value) ||
                !string.Equals(pair.Value, value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
