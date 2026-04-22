using System.Reflection;
using System.Net;
using System.Text;

namespace Turkcell.BT.Dotnet.Tests;

internal static class TestInfrastructure
{
    public static void InvokePrivateMethod(object instance, string methodName, params object?[]? parameters)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method?.Invoke(instance, parameters);
    }

    public static T? GetPrivateField<T>(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return (T?)field?.GetValue(instance);
    }
}

internal sealed class RouteHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> router) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return router(request);
    }

    public static HttpResponseMessage Json(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    public static HttpResponseMessage Text(HttpStatusCode statusCode, string text)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(text, Encoding.UTF8, "text/plain")
        };
    }
}
