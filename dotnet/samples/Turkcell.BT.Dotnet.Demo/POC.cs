namespace Turkcell.BT.Dotnet.Demo;

internal static class POC
{
    public static void PrintMinimalIntegrationExample()
    {
        Console.WriteLine("Minimal integration example:");
        Console.WriteLine("""
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddBeyondTrustSecrets();
            using var host = builder.Build();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            var managedPassword = configuration["bt.acc.<SystemName>.<AccountName>"];
            var secretPassword = configuration["bt.safe.<Folder>.<Title>.password"];
            """);
        Console.WriteLine();
    }
}
