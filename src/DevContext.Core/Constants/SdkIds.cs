namespace DevContext.Core.Constants;

/// <summary>Batch D (R2 §2.D) — the MSBuild SDK ids the engine reads as evidence, in ONE place.
/// They used to be string literals passed to four different <c>File.ReadAllText(csproj).Contains(…)</c>
/// probes (archetype detection, style detection, service-boundary inference), each with its own cache
/// and its own drift risk. Now the csproj is parsed once into <see cref="Contracts.ProjectInfo.Sdks"/>
/// and every consumer asks <see cref="Contracts.ProjectInfo.HasSdk"/> with an id from here.</summary>
public static class SdkIds
{
    /// <summary>ASP.NET Core web host — implies a runnable service even with no explicit OutputType.</summary>
    public const string Web = "Microsoft.NET.Sdk.Web";
    /// <summary>Worker Service host (a background-processing exe).</summary>
    public const string Worker = "Microsoft.NET.Sdk.Worker";
    /// <summary>Aspire AppHost — the orchestrator project. Declared as a child
    /// <c>&lt;Sdk Name="Aspire.AppHost.Sdk" /&gt;</c> ALONGSIDE the root SDK, which is exactly why the
    /// root-attribute-only <c>ParseSdk</c> could never see it.</summary>
    public const string AspireAppHost = "Aspire.AppHost.Sdk";
    /// <summary>Azure Functions host.</summary>
    public const string Functions = "Microsoft.NET.Sdk.Functions";
    /// <summary>WPF/WinForms desktop SDK (pre-net5 style).</summary>
    public const string WindowsDesktop = "Microsoft.NET.Sdk.WindowsDesktop";
    /// <summary>Builds no code: a solution-explorer HOLDER project (Prism D1.1b / audit E2).</summary>
    public const string NoTargets = "Microsoft.Build.NoTargets";
    /// <summary>Builds no code: an MSBuild traversal root (Prism D1.1b / audit E2).</summary>
    public const string Traversal = "Microsoft.Build.Traversal";
}
