package com.turkcell.bt.java;

import org.junit.jupiter.api.Test;
import static org.junit.jupiter.api.Assertions.*;

class DtoTest {
    @Test
    void testDtos() {
        // ManagedAccountDto
        ManagedAccountDto acc = new ManagedAccountDto();
        acc.setSystemName("Sys"); acc.setAccountName("Acc"); acc.setSystemId(1); acc.setAccountId(2);
        assertEquals("Sys", acc.getSystemName());
        assertEquals("Acc", acc.getAccountName());
        assertEquals(1, acc.getSystemId());
        assertEquals(2, acc.getAccountId());

        // SecretSafeItemDto (Coverage %0'dan %100'e çıkar)
        SecretSafeItemDto item = new SecretSafeItemDto();
        item.setTitle("T"); item.setFolder("F"); item.setPassword("P"); item.setUsername("U"); item.setAccount("A");
        assertEquals("T", item.getTitle());
        assertEquals("F", item.getFolder());
        assertEquals("P", item.getPassword());
        assertEquals("U", item.getUsername());
        assertEquals("A", item.getAccount());
    }
}