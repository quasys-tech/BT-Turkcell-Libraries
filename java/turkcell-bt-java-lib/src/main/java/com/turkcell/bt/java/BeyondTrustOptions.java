package com.turkcell.bt.java;

import com.fasterxml.jackson.annotation.JsonProperty;

public class BeyondTrustOptions {
    @JsonProperty("BEYONDTRUST_ENABLED")
    private boolean enabled = true;

    @JsonProperty("BEYONDTRUST_API_URL")
    private String apiUrl = "";

    @JsonProperty("BEYONDTRUST_API_KEY")
    private String apiKey = "";

    @JsonProperty("BEYONDTRUST_USE_APP_USER")
    private boolean useAppUser = false;
    private boolean useAppUserConfigured = false;

    @JsonProperty("BEYONDTRUST_CLIENT_ID")
    private String clientId;

    @JsonProperty("BEYONDTRUST_CLIENT_SECRET")
    private String clientSecret;

    @JsonProperty("BEYONDTRUST_RUNAS_USER")
    private String runAsUser;

    @JsonProperty("BEYONDTRUST_IGNORE_SSL_ERRORS")
    private boolean ignoreSslErrors;

    @JsonProperty("BEYONDTRUST_CERTIFICATE_CONTENT")
    private String certificateContent;

    @JsonProperty("BEYONDTRUST_REFRESH_INTERVAL")
    private int refreshIntervalSeconds = 1800;

    @JsonProperty("BEYONDTRUST_MANAGED_ACCOUNTS")
    private String managedAccounts;

    @JsonProperty("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED")
    private boolean allManagedAccountsEnabled;

    @JsonProperty("BEYONDTRUST_SECRET_SAFE_PATHS")
    private String secretSafePaths;

    @JsonProperty("BEYONDTRUST_ALL_SECRETS_ENABLED")
    private boolean allSecretsEnabled;

    public boolean isEnabled() { return enabled; }
    public void setEnabled(boolean enabled) { this.enabled = enabled; }
    public String getApiUrl() { return apiUrl; }
    public void setApiUrl(String apiUrl) { this.apiUrl = apiUrl; }
    public String getApiKey() { return apiKey; }
    public void setApiKey(String apiKey) { this.apiKey = apiKey; }
    public boolean isUseAppUser() { return useAppUser; }
    public void setUseAppUser(boolean useAppUser) {
        this.useAppUser = useAppUser;
        this.useAppUserConfigured = true;
    }
    boolean isUseAppUserConfigured() { return useAppUserConfigured; }
    public String getClientId() { return clientId; }
    public void setClientId(String clientId) { this.clientId = clientId; }
    public String getClientSecret() { return clientSecret; }
    public void setClientSecret(String clientSecret) { this.clientSecret = clientSecret; }
    public String getRunAsUser() { return runAsUser; }
    public void setRunAsUser(String runAsUser) { this.runAsUser = runAsUser; }
    public boolean isIgnoreSslErrors() { return ignoreSslErrors; }
    public void setIgnoreSslErrors(boolean ignoreSslErrors) { this.ignoreSslErrors = ignoreSslErrors; }
    public String getCertificateContent() { return certificateContent; }
    public void setCertificateContent(String certificateContent) { this.certificateContent = certificateContent; }
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

    public static BeyondTrustOptions fromEnv() {
        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setEnabled(readBoolean("BEYONDTRUST_ENABLED", true));
        options.setApiUrl(readString("BEYONDTRUST_API_URL"));
        options.setApiKey(readString("BEYONDTRUST_API_KEY"));
        Boolean useAppUser = readExplicitBoolean("BEYONDTRUST_USE_APP_USER");
        if (useAppUser != null) {
            options.setUseAppUser(useAppUser);
        }
        options.setClientId(readString("BEYONDTRUST_CLIENT_ID"));
        options.setClientSecret(readString("BEYONDTRUST_CLIENT_SECRET"));
        options.setRunAsUser(readString("BEYONDTRUST_RUNAS_USER"));
        options.setIgnoreSslErrors(readBoolean("BEYONDTRUST_IGNORE_SSL_ERRORS", false));
        options.setCertificateContent(readString("BEYONDTRUST_CERTIFICATE_CONTENT"));
        options.setManagedAccounts(readString("BEYONDTRUST_MANAGED_ACCOUNTS"));
        options.setAllManagedAccountsEnabled(readBoolean("BEYONDTRUST_ALL_MANAGED_ACCOUNTS_ENABLED", false));
        options.setSecretSafePaths(readString("BEYONDTRUST_SECRET_SAFE_PATHS"));
        options.setAllSecretsEnabled(readBoolean("BEYONDTRUST_ALL_SECRETS_ENABLED", false));
        options.setRefreshIntervalSeconds(readRefreshInterval());
        return options;
    }

    static String readString(String key) {
        if (System.getProperties().containsKey(key)) {
            return System.getProperty(key);
        }

        return System.getenv(key);
    }

    private static boolean readBoolean(String key, boolean defaultValue) {
        String value = readString(key);
        if (value == null || value.isBlank()) {
            return defaultValue;
        }

        return Boolean.parseBoolean(value);
    }

    private static Boolean readExplicitBoolean(String key) {
        String value = readString(key);
        if (value == null || value.isBlank()) {
            return null;
        }

        if ("true".equalsIgnoreCase(value) || "false".equalsIgnoreCase(value)) {
            return Boolean.parseBoolean(value);
        }

        throw new IllegalArgumentException("Invalid " + key + " value. Expected 'true' or 'false'.");
    }

    private static int readRefreshInterval() {
        String canonicalValue = readString("BEYONDTRUST_REFRESH_INTERVAL");
        if (canonicalValue != null) {
            Integer parsed = tryParseInteger(canonicalValue);
            return parsed != null ? parsed : 1800;
        }

        String legacyValue = readString("BT_REFRESH_TIME");
        if (legacyValue != null) {
            Integer parsed = tryParseInteger(legacyValue);
            return parsed != null ? parsed : 1800;
        }

        return 1800;
    }

    private static Integer tryParseInteger(String rawValue) {
        try {
            return Integer.parseInt(rawValue.trim());
        } catch (NumberFormatException ignored) {
            return null;
        }
    }
}
