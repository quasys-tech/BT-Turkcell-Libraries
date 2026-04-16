using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Turkcell.BT.Dotnet.Demo;

using IHost host = BuildHost(args);

var configuration = host.Services.GetRequiredService<IConfiguration>();
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

PrintBanner(configuration);
POC.PrintMinimalIntegrationExample();

var refreshInterval = ResolveRefreshInterval(configuration);
var previousSnapshotHash = string.Empty;

do
{
    var snapshot = GetBeyondTrustSnapshot(configuration);
    var currentSnapshotHash = string.Join("|", snapshot.Select(pair => $"{pair.Key}={pair.Value}"));

    if (!string.Equals(previousSnapshotHash, currentSnapshotHash, StringComparison.Ordinal))
    {
        Console.WriteLine();
        Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] Snapshot updated. {snapshot.Count} BeyondTrust key(s) loaded.");
        PrintAllKeys(snapshot);
        PrintSampleValues(configuration, snapshot);
        previousSnapshotHash = currentSnapshotHash;
    }

    if (refreshInterval <= 0)
    {
        break;
    }

    try
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
}
while (!cancellationTokenSource.IsCancellationRequested);

return;

static IHost BuildHost(string[] args)
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Configuration.AddBeyondTrustSecrets();
    return builder.Build();
}

static void PrintBanner(IConfiguration configuration)
{
    Console.WriteLine("============================================================");
    Console.WriteLine("DEMO ONLY - RAW SECRET LOGGING ENABLED - DO NOT USE THIS LOGGING STYLE IN PRODUCTION");
    Console.WriteLine("============================================================");
    Console.WriteLine($"Auth Mode : {(ResolveAuthMode(configuration) ? "OAuth / App User" : "Classic API")}");
    Console.WriteLine($"API Url   : {configuration["BEYONDTRUST_API_URL"] ?? "<not configured>"}");
    Console.WriteLine($"Refresh   : {ResolveRefreshInterval(configuration)} second(s)");
    Console.WriteLine();
}

static bool ResolveAuthMode(IConfiguration configuration)
{
    return bool.TryParse(configuration["BEYONDTRUST_USE_APP_USER"], out var useAppUser) && useAppUser;
}

static int ResolveRefreshInterval(IConfiguration configuration)
{
    return int.TryParse(configuration["BEYONDTRUST_REFRESH_INTERVAL"] ?? configuration["BT_REFRESH_TIME"], out var refreshInterval)
        ? refreshInterval
        : 1800;
}

static IReadOnlyList<KeyValuePair<string, string?>> GetBeyondTrustSnapshot(IConfiguration configuration)
{
    return configuration.AsEnumerable()
        .Where(pair => pair.Key.StartsWith("bt.", StringComparison.OrdinalIgnoreCase))
        .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
        .ToList();
}

static void PrintAllKeys(IReadOnlyList<KeyValuePair<string, string?>> snapshot)
{
    Console.WriteLine("--- Loaded bt.* keys ---");

    if (snapshot.Count == 0)
    {
        Console.WriteLine("No BeyondTrust keys are currently available. Check the required environment variables and API connectivity.");
    }
    else
    {
        foreach (var pair in snapshot)
        {
            Console.WriteLine($"{pair.Key} = {pair.Value}");
        }
    }

    Console.WriteLine("------------------------");
}

static void PrintSampleValues(IConfiguration configuration, IReadOnlyList<KeyValuePair<string, string?>> snapshot)
{
    var snapshotMap = snapshot.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    var lines = DemoExampleOutput.BuildExampleOutputLines(
        snapshotMap,
        configuration["BT_EXAMPLE_ACCOUNT"],
        configuration["BT_EXAMPLE_SAFE_PASSWORD"],
        configuration["BT_EXAMPLE_SAFE_USERNAME"]);

    foreach (var line in lines)
    {
        Console.WriteLine(line);
    }
}

namespace Turkcell.BT.Dotnet.Demo
{
    public static class DemoExampleOutput
    {
        public static IReadOnlyList<string> BuildExampleOutputLines(
            IReadOnlyDictionary<string, string?> snapshot,
            string? exampleAccountKey,
            string? exampleSafePasswordKey,
            string? exampleSafeUsernameKey)
        {
            var lines = new List<string>();

            AppendExampleOutput(lines, snapshot, "BT_EXAMPLE_ACCOUNT", "example account", "Managed Account Sample", exampleAccountKey);
            AppendExampleOutput(lines, snapshot, "BT_EXAMPLE_SAFE_PASSWORD", "example password", "Secret Safe Password Sample", exampleSafePasswordKey);
            AppendExampleOutput(lines, snapshot, "BT_EXAMPLE_SAFE_USERNAME", "example username", "Secret Safe Username Sample", exampleSafeUsernameKey);

            return lines;
        }

        private static void AppendExampleOutput(
            ICollection<string> lines,
            IReadOnlyDictionary<string, string?> snapshot,
            string parameterName,
            string friendlyName,
            string sampleLabel,
            string? configuredKey)
        {
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                lines.Add($"{parameterName} not set; skipping {friendlyName} output");
                return;
            }

            if (!snapshot.TryGetValue(configuredKey, out var value))
            {
                lines.Add($"Demo example key not found: {configuredKey}");
                return;
            }

            lines.Add($"{sampleLabel} ({configuredKey}) = {value}");
        }
    }
}
