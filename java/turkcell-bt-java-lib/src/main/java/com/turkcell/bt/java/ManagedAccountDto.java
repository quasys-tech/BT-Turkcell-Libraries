package com.turkcell.bt.java;

import com.fasterxml.jackson.annotation.JsonProperty;

class ManagedAccountDto {
    @JsonProperty("SystemID")
    private int systemId;

    @JsonProperty("SystemName")
    private String systemName = "";

    @JsonProperty("AccountID")
    private int accountId;

    @JsonProperty("AccountName")
    private String accountName = "";

    // Getter ve Setter'lar
    public int getSystemId() { return systemId; }
    public void setSystemId(int systemId) { this.systemId = systemId; }
    public String getSystemName() { return systemName; }
    public void setSystemName(String systemName) { this.systemName = systemName; }
    public int getAccountId() { return accountId; }
    public void setAccountId(int accountId) { this.accountId = accountId; }
    public String getAccountName() { return accountName; }
    public void setAccountName(String accountName) { this.accountName = accountName; }
}