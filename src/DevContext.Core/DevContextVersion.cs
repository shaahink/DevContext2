using System.Reflection;

namespace DevContext.Core;

public static class DevContextVersion
{
    public static string Display { get; } = GetDisplay();

    private static string GetDisplay()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(DevContextVersion).Assembly;
        var attr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attr is null) return "0.0.0";
        var version = attr.InformationalVersion;
        var plus = version.IndexOf('+');
        return plus > 0 ? version[..plus] : version;
    }
}
