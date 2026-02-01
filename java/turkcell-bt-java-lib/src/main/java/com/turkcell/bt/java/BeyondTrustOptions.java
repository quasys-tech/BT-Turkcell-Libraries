package com.turkcell.bt.java;

import com.fasterxml.jackson.annotation.JsonProperty;

public class BeyondTrustOptions {
    @JsonProperty("BEYONDTRUST_ENABLED")
    private boolean enabled = true;

    @JsonProperty("BEYONDTRUST_API_URL")
    private String apiUrl = "";

    @JsonProperty("BEYONDTRUST_API_KEY")
    private String apiKey = "";

    @JsonProperty("BEYONDTRUST_RUNAS_USER")
    private String runAsUser;

    @JsonProperty("BEYONDTRUST_IGNORE_SSL_ERRORS")
    private boolean ignoreSslErrors = false;

    @JsonProperty("BT_REFRESH_TIME")
    private int refreshIntervalSeconds = 1800;

    @JsonProperty("BEYONDTRUST_MANAGED_ACCOUNTS")
    private String managedAccounts;

    @JsonProperty("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED")
    private boolean allManagedAccountsEnabled = false;

    @JsonProperty("BEYONDTRUST_SECRET_SAFE_PATHS")
    private String secretSafePaths;

    @JsonProperty("BEYONDTRUST_ALL_SECRETS_ENABLED")
    private boolean allSecretsEnabled = false;

    // --- GETTER & SETTER ---
    public boolean isEnabled() { return enabled; }
    public void setEnabled(boolean enabled) { this.enabled = enabled; }
    public String getApiUrl() { return apiUrl; }
    public void setApiUrl(String apiUrl) { this.apiUrl = apiUrl; }
    public String getApiKey() { return apiKey; }
    public void setApiKey(String apiKey) { this.apiKey = apiKey; }
    public String getRunAsUser() { return runAsUser; }
    public void setRunAsUser(String runAsUser) { this.runAsUser = runAsUser; }
    public boolean isIgnoreSslErrors() { return ignoreSslErrors; }
    public void setIgnoreSslErrors(boolean ignoreSslErrors) { this.ignoreSslErrors = ignoreSslErrors; }
    public int getRefreshIntervalSeconds() { return refreshIntervalSeconds; }
    public void setRefreshIntervalSeconds(int refreshIntervalSeconds) { this.refreshIntervalSeconds = refreshIntervalSeconds; }
    public String getManagedAccounts() { return managedAccounts; }
    public void setManagedAccounts(String managedAccounts) { this.managedAccounts = managedAccounts; }
    public boolean isAllManagedAccountsEnabled() { return allManagedAccountsEnabled; }
    public void setAllManagedAccountsEnabled(boolean allManagedAccountsEnabled) { this.allManagedAccountsEnabled = allManagedAccountsEnabled; }
    public String getSecretSafePaths() { return secretSafePaths; }
    public void setSecretSafePaths(String secretSafePaths) { this.secretSafePaths = secretSafePaths; }
    public boolean isAllSecretsEnabled() { return allSecretsEnabled; }
    public void setAllSecretsEnabled(boolean allSecretsEnabled) { this.allSecretsEnabled = allSecretsEnabled; }

    // --- SİHİRLİ METOT: ENV OKUYUCU ---
    // ConfigMap'ten gelen değerleri otomatik okuyup nesneyi oluşturur.
    public static BeyondTrustOptions fromEnv() {
        BeyondTrustOptions options = new BeyondTrustOptions();

        // String Değerler (Null gelebilir, kütüphane içinde null check var)
        options.setApiUrl(System.getenv("BEYONDTRUST_API_URL"));
        options.setApiKey(System.getenv("BEYONDTRUST_API_KEY"));
        options.setRunAsUser(System.getenv("BEYONDTRUST_RUNAS_USER"));
        options.setManagedAccounts(System.getenv("BEYONDTRUST_MANAGED_ACCOUNTS"));
        options.setSecretSafePaths(System.getenv("BEYONDTRUST_SECRET_SAFE_PATHS"));

        // Boolean Değerler
        // ENABLED varsayılan olarak true olsun istiyorsak:
        String enabledEnv = System.getenv("BEYONDTRUST_ENABLED");
        options.setEnabled(enabledEnv == null || Boolean.parseBoolean(enabledEnv));

        options.setIgnoreSslErrors(Boolean.parseBoolean(System.getenv("BEYONDTRUST_IGNORE_SSL_ERRORS")));
        options.setAllManagedAccountsEnabled(Boolean.parseBoolean(System.getenv("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED")));
        options.setAllSecretsEnabled(Boolean.parseBoolean(System.getenv("BEYONDTRUST_ALL_SECRETS_ENABLED")));

        // Integer Değerler (Güvenli Parse)
        String refreshEnv = System.getenv("BT_REFRESH_TIME");
        if (refreshEnv != null && !refreshEnv.isBlank()) {
            try {
                options.setRefreshIntervalSeconds(Integer.parseInt(refreshEnv));
            } catch (NumberFormatException ignored) {
                // Parse edilemezse varsayılan (1800) kalır.
            }
        }

        return options;
    }
}