using Microsoft.Extensions.Configuration;

namespace Turkcell.BT.Dotnet.Lib;

/// <summary>
/// BeyondTrust entegrasyonu için gerekli konfigürasyon seçenekleri.
/// Turkcell kurumsal standartlarına uygun Environment değişkenleri ile eşleştirilmiştir.
/// </summary>
public class BeyondTrustOptions
{
    [ConfigurationKeyName("BEYONDTRUST_ENABLED")]
    public bool Enabled { get; set; } = true;

    [ConfigurationKeyName("BEYONDTRUST_API_URL")]
    public string ApiUrl { get; set; } = string.Empty;

    [ConfigurationKeyName("BEYONDTRUST_API_KEY")]
    public string ApiKey { get; set; } = string.Empty;

    // --- YENİ AUTH YÖNTEMİ PARAMETRELERİ ---
    [ConfigurationKeyName("BEYONDTRUST_USE_APP_USER")]
    public bool UseAppUser { get; set; } = true;

    [ConfigurationKeyName("BEYONDTRUST_CLIENT_ID")]
    public string? ClientId { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_CLIENT_SECRET")]
    public string? ClientSecret { get; set; }
    // ---------------------------------------

    [ConfigurationKeyName("BEYONDTRUST_RUNAS_USER")]
    public string? RunAsUser { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_IGNORE_SSL_ERRORS")]
    public bool IgnoreSslErrors { get; set; } = false;
    
    [ConfigurationKeyName("BEYONDTRUST_CERTIFICATE_CONTENT")]
    public string? CertificateContent { get; set; }

    /// <summary>
    /// Şifrelerin yenilenme periyodu (Saniye). Varsayılan 30 dakika (1800 sn).
    /// </summary>
    [ConfigurationKeyName("BEYONDTRUST_REFRESH_INTERVAL")]
    public int RefreshIntervalSeconds { get; set; } = 1800;

    [ConfigurationKeyName("BEYONDTRUST_MANAGED_ACCOUNTS")]
    public string? ManagedAccounts { get; set; } 

    [ConfigurationKeyName("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED")]
    public bool AllManagedAccountsEnabled { get; set; } = false;

    [ConfigurationKeyName("BEYONDTRUST_SECRET_SAFE_PATHS")]
    public string? SecretSafePaths { get; set; }

    [ConfigurationKeyName("BEYONDTRUST_ALL_SECRETS_ENABLED")]
    public bool AllSecretsEnabled { get; set; } = false;
}

/// <summary>
/// OAuth Token isteğinden dönen yanıt modeli.
/// </summary>
internal class TokenResponseDto
{
    public string? Access_Token { get; set; }
    public int Expires_In { get; set; }
    public string? Token_Type { get; set; }
    public string? Scope { get; set; }
}

/// <summary>
/// BeyondTrust Managed Account API'sinden gelen ham veri modeli.
/// </summary>
internal class ManagedAccountDto
{
    public int SystemId { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
}

/// <summary>
/// BeyondTrust Secret Safe API'sinden gelen ham veri modeli.
/// </summary>
internal class SecretSafeItemDto
{
    public string? Folder { get; set; }
    public string? Title { get; set; }
    public string? Username { get; set; }
    public string? Account { get; set; }
    public string? Password { get; set; }
    public string? SecretType { get; set; } // Sertifika veya Text tipi şifreler için kontrol
}