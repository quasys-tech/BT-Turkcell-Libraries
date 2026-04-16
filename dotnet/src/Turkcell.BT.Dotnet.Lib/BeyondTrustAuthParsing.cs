namespace Turkcell.BT.Dotnet.Lib;

internal sealed record ParsedApiKey(string Key, string? RunAsUser)
{
    public string ToAuthorizationHeader()
    {
        return string.IsNullOrWhiteSpace(RunAsUser)
            ? $"PS-Auth key={Key};"
            : $"PS-Auth key={Key}; runas={RunAsUser};";
    }
}

internal static class BeyondTrustAuthParsing
{
    public static bool TryParseApiKey(string? rawValue, string? explicitRunAsUser, out ParsedApiKey? parsedApiKey)
    {
        var candidate = (rawValue ?? string.Empty).Trim();
        if (candidate.StartsWith("PS-Auth", StringComparison.OrdinalIgnoreCase))
        {
            candidate = candidate["PS-Auth".Length..].Trim();
        }

        string? key = null;
        string? inlineRunAs = null;

        foreach (var part in candidate.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.StartsWith("key=", StringComparison.OrdinalIgnoreCase))
            {
                key = part[4..].Trim();
            }
            else if (part.StartsWith("runas=", StringComparison.OrdinalIgnoreCase))
            {
                inlineRunAs = part[6..].Trim();
            }
            else if (string.IsNullOrWhiteSpace(key))
            {
                key = part.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            parsedApiKey = null;
            return false;
        }

        var runAsUser = !string.IsNullOrWhiteSpace(explicitRunAsUser)
            ? explicitRunAsUser.Trim()
            : inlineRunAs;

        parsedApiKey = new ParsedApiKey(
            key.Trim(),
            string.IsNullOrWhiteSpace(runAsUser) ? null : runAsUser.Trim());

        return true;
    }
}
