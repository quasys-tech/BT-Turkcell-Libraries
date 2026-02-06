using System.Reflection;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Turkcell.BT.Dotnet.Lib;

namespace Turkcell.BT.Dotnet.Tests;

internal static class TestInfrastructure
{
    public static T? GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return (T?)field?.GetValue(instance);
    }

    public static void InjectMockClient(BeyondTrustService service, HttpClient httpClient)
    {
        var field = typeof(BeyondTrustService).GetField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(service, httpClient);
    }

    public static void ClearStaticCache()
    {
        var field = typeof(BeyondTrustService).GetField("_passwordCache", BindingFlags.Static | BindingFlags.NonPublic);
        var dict = (ConcurrentDictionary<string, string>)field?.GetValue(null)!;
        dict?.Clear();
    }
}

internal sealed class RouteHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> router) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => router(request);

    public static HttpResponseMessage Json(HttpStatusCode code, string json)
        => new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage Text(HttpStatusCode code, string text)
        => new(code) { Content = new StringContent(text, Encoding.UTF8, "text/plain") };
}