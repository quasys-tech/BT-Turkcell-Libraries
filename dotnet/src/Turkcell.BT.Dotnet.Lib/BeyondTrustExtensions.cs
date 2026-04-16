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
        var options = new BeyondTrustOptions();
        configuration.Bind(options);

        if (options.RefreshIntervalSeconds <= 0 &&
            int.TryParse(configuration["BT_REFRESH_TIME"], out var legacyRefreshInterval))
        {
            options.RefreshIntervalSeconds = legacyRefreshInterval;
        }

        return options;
    }
}

internal static class BeyondTrustConfigurationValidation
{
    public static BeyondTrustValidationResult Validate(IConfiguration configuration, BeyondTrustOptions options)
    {
        var missingSettings = new List<string>();

        if (string.IsNullOrWhiteSpace(configuration["BEYONDTRUST_USE_APP_USER"]))
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
