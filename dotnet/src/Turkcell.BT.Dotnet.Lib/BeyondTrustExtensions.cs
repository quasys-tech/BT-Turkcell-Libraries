using Microsoft.Extensions.Configuration;
using Turkcell.BT.Dotnet.Lib;

namespace Microsoft.Extensions.Configuration;

public static class BeyondTrustExtensions
{
    public static IConfigurationBuilder AddBeyondTrustSecrets(this IConfigurationBuilder builder)
    {
        var existingConfiguration = builder.Build();
        var options = BindOptions(existingConfiguration);
        var validation = BeyondTrustConfigurationValidation.Validate(existingConfiguration, options);

        if (!options.Enabled)
        {
            Console.WriteLine("[BeyondTrust] Configuration provider is disabled because BEYONDTRUST_ENABLED=false.");
            return builder;
        }

        if (!validation.IsValid)
        {
            Console.WriteLine("[BeyondTrust] Configuration provider was not added because required settings are missing.");
            foreach (var missingSetting in validation.MissingSettings)
            {
                Console.WriteLine($"[BeyondTrust] Missing setting: {missingSetting}");
            }

            return builder;
        }

        builder.Add(new BeyondTrustConfigurationSource(options));

        var authMode = options.UseAppUser ? "OAuth / App User" : "Classic API";
        Console.WriteLine($"[BeyondTrust] Configuration provider added. Auth mode: {authMode}. Refresh interval: {options.RefreshIntervalSeconds}s.");

        if (options.AllSecretsEnabled)
        {
            Console.WriteLine("[BeyondTrust] BEYONDTRUST_ALL_SECRETS_ENABLED is accepted for compatibility, but this version still loads Secret Safe entries by BEYONDTRUST_SECRET_SAFE_PATHS.");
        }

        return builder;
    }

    internal static BeyondTrustOptions BindOptions(IConfiguration configuration)
    {
        var options = new BeyondTrustOptions
        {
            Enabled = ReadBoolean(configuration, "BEYONDTRUST_ENABLED", true),
            ApiUrl = configuration["BEYONDTRUST_API_URL"] ?? string.Empty,
            ApiKey = configuration["BEYONDTRUST_API_KEY"] ?? string.Empty,
            ClientId = configuration["BEYONDTRUST_CLIENT_ID"],
            ClientSecret = configuration["BEYONDTRUST_CLIENT_SECRET"],
            RunAsUser = configuration["BEYONDTRUST_RUNAS_USER"],
            IgnoreSslErrors = ReadBoolean(configuration, "BEYONDTRUST_IGNORE_SSL_ERRORS", false),
            CertificateContent = configuration["BEYONDTRUST_CERTIFICATE_CONTENT"],
            RefreshIntervalSeconds = ResolveRefreshInterval(configuration),
            ManagedAccounts = configuration["BEYONDTRUST_MANAGED_ACCOUNTS"],
            AllManagedAccountsEnabled = ReadBoolean(configuration, "BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", false),
            SecretSafePaths = configuration["BEYONDTRUST_SECRET_SAFE_PATHS"],
            AllSecretsEnabled = ReadBoolean(configuration, "BEYONDTRUST_ALL_SECRETS_ENABLED", false)
        };

        var useAppUserValue = configuration["BEYONDTRUST_USE_APP_USER"];
        if (!string.IsNullOrWhiteSpace(useAppUserValue))
        {
            options.UseAppUser = ReadBoolean(configuration, "BEYONDTRUST_USE_APP_USER", false);
        }

        return options;
    }

    internal static int ResolveRefreshInterval(IConfiguration configuration)
    {
        var canonicalValue = configuration["BEYONDTRUST_REFRESH_INTERVAL"];
        if (!string.IsNullOrWhiteSpace(canonicalValue))
        {
            if (int.TryParse(canonicalValue, out var refreshInterval))
            {
                return refreshInterval;
            }

            throw new InvalidOperationException("Invalid BEYONDTRUST_REFRESH_INTERVAL value. Expected an integer number of seconds.");
        }

        var legacyValue = configuration["BT_REFRESH_TIME"];
        if (!string.IsNullOrWhiteSpace(legacyValue) &&
            int.TryParse(legacyValue, out var legacyRefreshInterval))
        {
            return legacyRefreshInterval;
        }

        return BeyondTrustOptions.DefaultRefreshIntervalSeconds;
    }

    private static bool ReadBoolean(IConfiguration configuration, string key, bool defaultValue)
    {
        var rawValue = configuration[key];
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return defaultValue;
        }

        if (bool.TryParse(rawValue, out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException($"Invalid {key} value. Expected 'true' or 'false'.");
    }
}

internal static class BeyondTrustConfigurationValidation
{
    public static BeyondTrustValidationResult Validate(IConfiguration configuration, BeyondTrustOptions options)
    {
        var missingSettings = new List<string>();

        if (!options.UseAppUserConfigured)
        {
            missingSettings.Add("BEYONDTRUST_USE_APP_USER");
        }

        if (string.IsNullOrWhiteSpace(options.ApiUrl))
        {
            missingSettings.Add("BEYONDTRUST_API_URL");
        }

        if (options.UseAppUser)
        {
            if (string.IsNullOrWhiteSpace(options.ClientId))
            {
                missingSettings.Add("BEYONDTRUST_CLIENT_ID");
            }

            if (string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                missingSettings.Add("BEYONDTRUST_CLIENT_SECRET");
            }
        }
        else if (!BeyondTrustAuthParsing.TryParseApiKey(options.ApiKey, options.RunAsUser, out _))
        {
            missingSettings.Add("BEYONDTRUST_API_KEY");
        }

        return new BeyondTrustValidationResult(missingSettings.Count == 0, missingSettings);
    }
}

internal sealed record BeyondTrustValidationResult(bool IsValid, IReadOnlyList<string> MissingSettings);
