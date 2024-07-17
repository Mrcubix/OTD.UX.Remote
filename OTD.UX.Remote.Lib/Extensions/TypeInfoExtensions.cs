using System.Reflection;

namespace OTD.UX.Remote.Lib.Extensions;

public static class TypeExtensions
{
    public static T GetValue<T>(this Type typeInfo, object instance, string name)
        where T : class
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return typeInfo
                   .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .GetValue(instance) as T;
    }

    public static T GetStaticValue<T>(this Type typeInfo, string name)
        where T : class
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        return typeInfo
                   .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                   .GetValue(null) as T;
    }

    public static void SetValue(this Type typeInfo, object instance, string name, object value)
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)   
            throw new ArgumentNullException(nameof(name));

        typeInfo
            .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(instance, value);
    }

    public static void SetPropertyValue(this Type typeInfo, object instance, string name, object value)
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        typeInfo
            .GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(instance, value);
    }

    public static void SetStaticValue(this Type typeInfo, string name, object value)
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        typeInfo
            .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, value);
    }

    public static void SetStaticPropertyValue(this Type typeInfo, string name, object value)
    {
        if (typeInfo == null)
            throw new ArgumentNullException(nameof(typeInfo));

        if (name == null)
            throw new ArgumentNullException(nameof(name));

        typeInfo
            .GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, value);
    }
}