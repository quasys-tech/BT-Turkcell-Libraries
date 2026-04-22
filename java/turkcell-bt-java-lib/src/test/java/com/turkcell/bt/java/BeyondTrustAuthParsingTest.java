package com.turkcell.bt.java;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class BeyondTrustAuthParsingTest {

    @Test
    @DisplayName("parseApiKey PS-Auth prefix ve inline runas degerini ayrisabilmeli")
    void parseApiKeySupportsPrefixAndInlineRunAs() {
        BeyondTrustAuthParsing.ParsedApiKey parsed =
                BeyondTrustAuthParsing.parseApiKey("PS-Auth key=demo-key; runas=svc-demo;", null);

        assertNotNull(parsed);
        assertEquals("PS-Auth key=demo-key; runas=svc-demo;", parsed.toAuthorizationHeader());
    }

    @Test
    @DisplayName("parseApiKey explicit runas verildiginde inline runas bilgisini ezmeli")
    void parseApiKeyExplicitRunAsOverridesInlineRunAs() {
        BeyondTrustAuthParsing.ParsedApiKey parsed =
                BeyondTrustAuthParsing.parseApiKey("key=demo-key; runas=inline-user;", "explicit-user");

        assertNotNull(parsed);
        assertEquals("PS-Auth key=demo-key; runas=explicit-user;", parsed.toAuthorizationHeader());
    }

    @Test
    @DisplayName("parseApiKey raw key formatini kabul etmeli ve bos degerde null donmeli")
    void parseApiKeySupportsRawKeyAndBlankValue() {
        BeyondTrustAuthParsing.ParsedApiKey parsed =
                BeyondTrustAuthParsing.parseApiKey("demo-key", null);

        assertNotNull(parsed);
        assertEquals("PS-Auth key=demo-key;", parsed.toAuthorizationHeader());
        assertNull(BeyondTrustAuthParsing.parseApiKey("   ", null));
        assertNull(BeyondTrustAuthParsing.parseApiKey(null, null));
    }
}
