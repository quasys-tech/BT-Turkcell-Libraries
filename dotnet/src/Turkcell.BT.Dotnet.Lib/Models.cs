using Microsoft.Extensions.Configuration;

namespace Turkcell.BT.Dotnet.Lib;

/// <summary>
/// Canonical BeyondTrust configuration model for the .NET library.
/// </summary>
public sealed class BeyondTrustOptions
{
    public const int DefaultRefreshIntervalSeconds = 1800;

    private bool _useAppUser;

    [ConfigurationKeyName("BEYONDTRUST_ENABLED")]
    public bool Enabled { get; set; } = true;

    [ConfigurationKeyName("BEYONDTRUST_API_URL")]
    public string ApiUrl { get; set; } = string.Empty;

    [ConfigurationKeyName("BEYONDTRUST_API_KEY")]
    public string ApiKey { get; set; } = string.Empty;

    [ConfigurationKeyName("BEYONDTRUST_USE_APP_USER")]
    public bool UseAppUser
    {
        get => _useAppUser;
        set
        {
            _useAppUser = value;
            UseAppUserConfigured = true;
        }
    }

    internal bool UseAppUserConfigured { get; private set; }

    [ConfigurationKeyName("BEYONDTRUST_CLIENT_ID")]
    public string? ClientId { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_CLIENT_SECRET")]
    public string? ClientSecret { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_RUNAS_USER")]
    public string? RunAsUser { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_IGNORE_SSL_ERRORS")]
    public bool IgnoreSslErrors { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_CERTIFICATE_CONTENT")]
    public string? CertificateContent { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_REFRESH_INTERVAL")]
    public int RefreshIntervalSeconds { get; set; } = DefaultRefreshIntervalSeconds;

    [ConfigurationKeyName("BEYONDTRUST_MANAGED_ACCOUNTS")]
    public string? ManagedAccounts { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED")]
    public bool AllManagedAccountsEnabled { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_SECRET_SAFE_PATHS")]
    public string? SecretSafePaths { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_ALL_SECRETS_ENABLED")]
    public bool AllSecretsEnabled { get; set; }
}

internal sealed class TokenResponseDto
{
    public string? Access_Token { get; set; }
    public int Expires_In { get; set; }
    public string? Token_Type { get; set; }
    public string? Scope { get; set; }
}

internal sealed class ManagedAccountDto
{
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
}

internal sealed class SecretSafeItemDto
{
    public string? Folder { get; set; }
    public string? Title { get; set; }
    public string? Username { get; set; }
    public string? Account { get; set; }
    public string? Password { get; set; }
    public string? SecretType { get; set; }
}
