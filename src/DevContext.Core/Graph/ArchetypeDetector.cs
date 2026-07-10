namespace DevContext.Core.Graph;

using DevContext.Core.Graph.EntrySurfaces;

/// <summary>
/// What kind of codebase this is — independent of architecture <c>Style</c>. An <see cref="App"/> has
/// application entry points (HTTP/bus/hosted/scheduled); a <see cref="Library"/> is a packable component
/// with a public API and no entry points (e.g. AutoMapper); <see cref="Worker"/> is a background-service
/// app with no web endpoints; <see cref="Blazor"/> is a Blazor WASM/Server app with component pages.
/// The archetype decides which renderer runs. (L7.2 — Worker + Blazor added.)
/// </summary>
public enum Archetype { App, Library, Gateway, Desktop, Worker, Blazor }

/// <summary>Decides <see cref="Archetype"/> from the entry inventory + project shape.</summary>
public static class ArchetypeDetector
{
    private static readonly EntryPointKind[] AppEntryKinds = EntrySurfaceCatalog.All
        .Where(d => d.Kind is not null && d.Role == SurfaceRole.AppEntry)
        .Select(d => d.Kind!.Value)
        .Distinct()
        .ToArray();

    /// <summary>Library ⇔ no application entry points AND a non-executable project with a public
    /// surface exists, where any executable projects are merely auxiliary samples/benchmarks that
    /// reference the library (so AutoMapper's Benchmark/TestApp don't flip it to App).
    /// Gateway ⇔ Ocelot/YARP reverse-proxy packages detected (overrides App/Library). App otherwise.</summary>
    /// <summary>Decides <see cref="Archetype"/> from the entry inventory + project shape.
    /// M1.9: gateway signal alone no longer forces Gateway archetype when multiple services exist —
    /// the Gateway is a role within a microservices app, not the solution archetype.
    /// L7.2: Worker and Blazor detected as App subtypes based on entry-point shape.</summary>
    public static Archetype Detect(DiscoveryModel model, ImmutableArray<EntryPoint> entries)
    {
        // M1.9: gateway with multiple services → microservices App, not Gateway alone
        if (model.Architecture.Has(ArchitectureSignals.Keys.Gateway))
        {
            var runnableSvcCount = model.Projects.Count(p =>
                IsRunnableService(p) && !p.PackageReferences.Any(pr =>
                    pr.Name.Contains("Yarp", StringComparison.OrdinalIgnoreCase)
                    || pr.Name.Contains("Ocelot", StringComparison.OrdinalIgnoreCase)));
            if (runnableSvcCount >= 2)
                return Archetype.App; // gateway is a role within microservices
            return Archetype.Gateway; // single service behind gateway — Gateway archetype
        }

        // F1: Framework libraries (SignalR, gRPC, MassTransit, Orleans, etc.) are
        // libraries only when the signal is self-sourced (ProjectName/ProjectReference).
        if (IsSelfSourcedFrameworkSignal(model))
            return Archetype.Library;

        // A library's sample/snippet apps (e.g. a Minimal-API demo of the library) are not the library —
        // ignore their entries and projects so they don't flip the archetype to App.
        if (!entries.IsDefaultOrEmpty && entries.Any(e =>
            AppEntryKinds.Contains(e.Kind)
            && !(e.Provenance is { } prov && ProjectClassifier.IsSamplePath(prov))))
            return DetectAppSubtype(model, entries);

        var classifier = new ProjectClassifier(model.Projects);
        var nonTest = model.Projects
            .Where(p => !classifier.IsInTestProject(p.FilePath))
            .Where(p => !ProjectClassifier.IsSamplePath(p.FilePath))
            .ToList();
        if (nonTest.Count == 0)
            return Archetype.App;

        static bool IsExe(ProjectInfo p) => p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var exe = nonTest.Where(IsExe).ToList();
        var nonExe = nonTest.Where(p => !IsExe(p)).ToList();
        if (nonExe.Count == 0)
            return Archetype.App; // pure executable(s)

        var libNames = nonExe.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allExeAreAuxiliary = exe.All(e =>
            ProjectClassifier.IsSamplePath(e.FilePath)
            || ProjectClassifier.IsTestPath(e.FilePath)
            || e.OutputType?.Contains("WinExe", StringComparison.OrdinalIgnoreCase) != true
                && e.ProjectReferences.Any(r => libNames.Contains(Path.GetFileNameWithoutExtension(r))));
        if (!allExeAreAuxiliary)
            return Archetype.App; // a standalone executable that isn't just a sample of the library

        var packable = nonExe.Any(p => p.IsPackable);
        var hasPublicSurface = model.Types.Values.Any(t =>
            t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
            && !classifier.IsInTestProject(t.FilePath)
            && !ProjectClassifier.IsSamplePath(t.FilePath));

        return packable || hasPublicSurface ? Archetype.Library : Archetype.App;
    }

    /// <summary>L7.2 — when the solution is App, refine to Worker, Blazor, or Desktop based on entry-point shape.</summary>
    private static Archetype DetectAppSubtype(DiscoveryModel model, ImmutableArray<EntryPoint> entries)
    {
        var hasBlazor = model.Architecture.Has(ArchitectureSignals.Keys.Blazor);
        var hasControllers = model.Architecture.Has(ArchitectureSignals.Keys.Controllers);
        var hasMinimalApis = model.Architecture.Has(ArchitectureSignals.Keys.MinimalApis);
        var hasDesktop = model.Architecture.Has(ArchitectureSignals.Keys.DesktopUi);
        var hasWebEndpoints = hasControllers || hasMinimalApis
            || model.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints)
            || model.Architecture.Has(ArchitectureSignals.Keys.RazorPages);

        // Desktop: WindowsDesktop SDK or DesktopUi signal, no web endpoint frameworks
        if (hasDesktop && !hasWebEndpoints && !hasBlazor)
            return Archetype.Desktop;

        // Blazor: Blazor signal present, no traditional web endpoint frameworks
        if (hasBlazor && !hasWebEndpoints)
            return Archetype.Blazor;

        // Worker: dominated by hosted-service/scheduled entries with NO HTTP/gRPC endpoints
        var appEntries = entries.Where(e =>
            AppEntryKinds.Contains(e.Kind)
            && !(e.Provenance is { } prov && ProjectClassifier.IsSamplePath(prov)))
            .ToList();
        var webEntryCount = appEntries.Count(e =>
            e.Kind is EntryPointKind.HttpEndpoint or EntryPointKind.GrpcService
                 or EntryPointKind.SignalRHub or EntryPointKind.GraphQlField);
        var workerEntryCount = appEntries.Count(e =>
            e.Kind is EntryPointKind.HostedService or EntryPointKind.ScheduledJob);

        if (webEntryCount == 0 && workerEntryCount > 0)
            return Archetype.Worker;

        return Archetype.App;
    }

    /// <summary>M1.9 — true when the project is a runnable service (Exe or web SDK, not a library).</summary>
    private static bool IsRunnableService(ProjectInfo p)
    {
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var isWebSdk = p.PackageReferences.Any(pr => pr.Name.Contains("AspNetCore", StringComparison.OrdinalIgnoreCase));
        var hasWebServer = p.FilePath is { } cp && IsWebSdkProject(cp);
        return isExe || isWebSdk || hasWebServer;
    }

    private static bool IsWebSdkProject(string csprojPath)
    {
        try
        {
            var text = File.ReadAllText(csprojPath);
            return text.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    // Framework-library signals that, when self-sourced (ProjectName/ProjectReference), mean
    // this repo IS the framework itself — not a consumer app. PackageReference/ProjectSdk sources
    // indicate a consumer app and do NOT force Library.
    // A signal can be both an AppEntry surface (when package-sourced) AND a framework-library
    // indicator (when self-sourced via SelfNamePatterns).
    private static readonly string[] LibraryFrameworkSignals = EntrySurfaceCatalog.All
        .Where(d => d.SignalKey.Length > 0
            && (d.Role == SurfaceRole.FrameworkLibrary || d.SelfNamePatterns.Length > 0))
        .Select(d => d.SignalKey)
        .Distinct()
        .ToArray();

    private static bool IsSelfSourcedFrameworkSignal(DiscoveryModel model)
    {
        foreach (var sig in LibraryFrameworkSignals)
        {
            var signal = model.Architecture.Get(sig);
            if (signal is { Detected: true } && signal.DetectedVia is "ProjectName" or "ProjectReference")
                return true;
        }
        return false;
    }
}
