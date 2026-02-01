using System.Reflection;

namespace Turkcell.BT.Dotnet.Tests;

public static class TestReflection
{
    public static T? GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T?)field?.GetValue(obj);
    }
    
    public static T? GetStaticField<T>(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        return (T?)field?.GetValue(null);
    }
}