package com.turkcell.bt.java.demo;

import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertTrue;

class BTDemoAppTest {

    @Test
    @DisplayName("BT_EXAMPLE_SAFE_USERNAME set edildiginde demo raw username output uretmeli")
    void buildExampleOutputLinesLogsConfiguredSafeUsernameSample() {
        Map<String, String> snapshot = new LinkedHashMap<>();
        snapshot.put("bt.safe.SampleFolder.SampleTitle.username", "demo-user");

        List<String> lines = BTDemoApp.buildExampleOutputLines(
                snapshot,
                null,
                null,
                "bt.safe.SampleFolder.SampleTitle.username");

        assertTrue(lines.contains("Secret Safe Username Sample (bt.safe.SampleFolder.SampleTitle.username) = demo-user"));
    }

    @Test
    @DisplayName("BT_EXAMPLE_SAFE_USERNAME yoksa demo acik skip mesaji vermeli")
    void buildExampleOutputLinesShowsSkipMessageWhenSafeUsernameExampleIsNotSet() {
        List<String> lines = BTDemoApp.buildExampleOutputLines(
                new LinkedHashMap<>(),
                null,
                null,
                null);

        assertTrue(lines.contains("BT_EXAMPLE_SAFE_USERNAME not set; skipping example username output"));
    }
}
