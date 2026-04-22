package com.turkcell.bt.java.demo;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertTrue;

class BTDemoAppTest {

    @Test
    @DisplayName("POC output blogu configured safe username sample icermeli")
    void buildOutputBlockIncludesConfiguredSafeUsernameSample() {
        String output = POC.buildOutputBlock(
                "bt.acc.SampleSystem.SampleAccount",
                "demo-password",
                "bt.safe.SampleFolder.SampleTitle.password",
                "demo-secret",
                "bt.safe.SampleFolder.SampleTitle.username",
                "demo-user");

        assertTrue(output.contains("Secret Safe Username Sample (bt.safe.SampleFolder.SampleTitle.username) = demo-user"));
    }

    @Test
    @DisplayName("POC output blogu configured account sample icermeli")
    void buildOutputBlockIncludesConfiguredManagedAccountSample() {
        String output = POC.buildOutputBlock(
                "bt.acc.SampleSystem.SampleAccount",
                "demo-password",
                "bt.safe.SampleFolder.SampleTitle.password",
                "demo-secret",
                "bt.safe.SampleFolder.SampleTitle.username",
                "demo-user");

        assertTrue(output.contains("Managed Account Sample (bt.acc.SampleSystem.SampleAccount) = demo-password"));
    }
}
