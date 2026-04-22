using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Turkcell.BT.Dotnet.Demo;

internal static class POC
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddBeyondTrustSecrets();

        using var host = builder.Build();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        var exampleAccountKey = (Environment.GetEnvironmentVariable("BT_EXAMPLE_ACCOUNT") ?? string.Empty).Trim();
        var exampleSafePasswordKey = (Environment.GetEnvironmentVariable("BT_EXAMPLE_SAFE_PASSWORD") ?? string.Empty).Trim();
        var exampleSafeUsernameKey = (Environment.GetEnvironmentVariable("BT_EXAMPLE_SAFE_USERNAME") ?? string.Empty).Trim();

        var previousOutput = string.Empty;

        while (true)
        {
            var currentOutput = $"""
                Managed Account Sample ({exampleAccountKey}) = {configuration[exampleAccountKey]}
                Secret Safe Password Sample ({exampleSafePasswordKey}) = {configuration[exampleSafePasswordKey]}
                Secret Safe Username Sample ({exampleSafeUsernameKey}) = {configuration[exampleSafeUsernameKey]}
                """;

            if (!string.Equals(previousOutput, currentOutput, StringComparison.Ordinal))
            {
                Console.Write(currentOutput);
                previousOutput = currentOutput;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
