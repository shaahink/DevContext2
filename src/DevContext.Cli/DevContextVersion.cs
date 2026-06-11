using System.Reflection;

namespace DevContext.Cli;

public static class DevContextVersion
{
    public static string Display { get; } = GetDisplay();

    private static string GetDisplay()
    {
        var attr = typeof(DevContextVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attr is null) return "0.0.0";
        // MinVer produces "2.1.0+abc1234" — strip the commit hash for display
        var version = attr.InformationalVersion;
        var plus = version.IndexOf('+');
        return plus > 0 ? version[..plus] : version;
    }
}
