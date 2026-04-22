package com.turkcell.bt.java;

final class BeyondTrustAuthParsing {
    private BeyondTrustAuthParsing() {
    }

    static ParsedApiKey parseApiKey(String rawValue, String explicitRunAsUser) {
        String candidate = rawValue == null ? "" : rawValue.trim();
        if (candidate.regionMatches(true, 0, "PS-Auth", 0, "PS-Auth".length())) {
            candidate = candidate.substring("PS-Auth".length()).trim();
        }

        String key = null;
        String inlineRunAs = null;

        for (String part : candidate.split(";")) {
            String trimmed = part.trim();
            if (trimmed.isEmpty()) {
                continue;
            }

            if (trimmed.regionMatches(true, 0, "key=", 0, 4)) {
                key = trimmed.substring(4).trim();
            } else if (trimmed.regionMatches(true, 0, "runas=", 0, 6)) {
                inlineRunAs = trimmed.substring(6).trim();
            } else if (key == null || key.isBlank()) {
                key = trimmed;
            }
        }

        if (key == null || key.isBlank()) {
            return null;
        }

        String runAsUser = explicitRunAsUser != null && !explicitRunAsUser.isBlank()
                ? explicitRunAsUser.trim()
                : inlineRunAs;

        return new ParsedApiKey(key.trim(), runAsUser == null || runAsUser.isBlank() ? null : runAsUser.trim());
    }

    static final class ParsedApiKey {
        private final String key;
        private final String runAsUser;

        ParsedApiKey(String key, String runAsUser) {
            this.key = key;
            this.runAsUser = runAsUser;
        }

        String toAuthorizationHeader() {
            return runAsUser == null || runAsUser.isBlank()
                    ? "PS-Auth key=" + key + ";"
                    : "PS-Auth key=" + key + "; runas=" + runAsUser + ";";
        }
    }
}
