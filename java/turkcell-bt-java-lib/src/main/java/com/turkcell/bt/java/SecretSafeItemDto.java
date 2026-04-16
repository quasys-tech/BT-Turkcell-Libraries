package com.turkcell.bt.java;

import com.fasterxml.jackson.annotation.JsonProperty;

class SecretSafeItemDto {
    @JsonProperty("Folder")
    private String folder;

    @JsonProperty("Title")
    private String title;

    @JsonProperty("Username")
    private String username;

    @JsonProperty("Account")
    private String account;

    @JsonProperty("Password")
    private String password;

    @JsonProperty("SecretType")
    private String secretType;

    public String getFolder() { return folder; }
    public void setFolder(String folder) { this.folder = folder; }
    public String getTitle() { return title; }
    public void setTitle(String title) { this.title = title; }
    public String getUsername() { return username; }
    public void setUsername(String username) { this.username = username; }
    public String getAccount() { return account; }
    public void setAccount(String account) { this.account = account; }
    public String getPassword() { return password; }
    public void setPassword(String password) { this.password = password; }
    public String getSecretType() { return secretType; }
    public void setSecretType(String secretType) { this.secretType = secretType; }
}
