package com.turkcell.bt.java;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class DtoTest {
    @Test
    void dtoAndOptionAccessorsWork() {
        ManagedAccountDto account = new ManagedAccountDto();
        account.setSystemName("Sys");
        account.setAccountName("Acc");
        account.setSystemId(1);
        account.setAccountId(2);
        assertEquals("Sys", account.getSystemName());
        assertEquals("Acc", account.getAccountName());
        assertEquals(1, account.getSystemId());
        assertEquals(2, account.getAccountId());

        SecretSafeItemDto item = new SecretSafeItemDto();
        item.setTitle("Title");
        item.setFolder("Folder");
        item.setPassword("Password");
        item.setUsername("Username");
        item.setAccount("Account");
        item.setSecretType("Credential");
        assertEquals("Title", item.getTitle());
        assertEquals("Folder", item.getFolder());
        assertEquals("Password", item.getPassword());
        assertEquals("Username", item.getUsername());
        assertEquals("Account", item.getAccount());
        assertEquals("Credential", item.getSecretType());

        BeyondTrustOptions options = new BeyondTrustOptions();
        options.setUseAppUser(true);
        options.setClientId("client-id");
        options.setClientSecret("client-secret");
        options.setCertificateContent("pem");
        options.setRefreshIntervalSeconds(15);
        assertTrue(options.isUseAppUser());
        assertEquals("client-id", options.getClientId());
        assertEquals("client-secret", options.getClientSecret());
        assertEquals("pem", options.getCertificateContent());
        assertEquals(15, options.getRefreshIntervalSeconds());
    }
}
