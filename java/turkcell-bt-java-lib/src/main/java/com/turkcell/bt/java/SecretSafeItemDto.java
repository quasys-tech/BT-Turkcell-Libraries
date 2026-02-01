package com.turkcell.bt.java;

import com.fasterxml.jackson.annotation.JsonProperty;

class SecretSafeItemDto {
    private String folder;
    private String title;
    private String username;
    private String account;
    private String password;
    private String secretType;

    // Getter ve Setter'lar
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