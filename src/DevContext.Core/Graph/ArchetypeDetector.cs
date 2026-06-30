namespace DevContext.Core.Graph;

/// <summary>
/// What kind of codebase this is — independent of architecture <c>Style</c>. An <see cref="App"/> has
/// application entry points (HTTP/bus/hosted/scheduled); a <see cref="Library"/> is a packable component
/// with a public API and no entry points (e.g. AutoMapper). The archetype decides which renderer runs:
/// the entry-point Map vs the capability-grouped public surface (assessment G3).
/// </summary>
public enum Archetype { App, Library, Gateway }

/// <summary>Decides <see cref="Archetype"/> from the entry inventory + project shape.</summary>
public static class ArchetypeDetector
{
    private static readonly EntryPointKind[] AppEntryKinds =
    [
        EntryPointKind.HttpEndpoint, EntryPointKind.MessageConsumer,
        EntryPointKind.HostedService, EntryPointKind.ScheduledJob,
        EntryPointKind.UiEntry,
    ];

    /// <summary>Library ⇔ no application entry points AND a non-executable project with a public
    /// surface exists, where any executable projects are merely auxiliary samples/benchmarks that
    /// reference the library (so AutoMapper's Benchmark/TestApp don't flip it to App).
    /// Gateway ⇔ Ocelot/YARP reverse-proxy packages detected (overrides App/Library). App otherwise.</summary>
    public static Archetype Detect(DiscoveryModel model, ImmutableArray<EntryPoint> entries)
    {
        // W7: gateway packages (Ocelot, Microsoft.ReverseProxy) → Gateway archetype
        if (model.Architecture.Has(ArchitectureSignals.Keys.Gateway))
            return Archetype.Gateway;

        // F1: Framework libraries (SignalR, gRPC, MassTransit, Orleans, etc.) are
        // libraries regardless of whether their test hosts or internal management APIs
        // create application entry points. Check BEFORE entry inspection.
        if (IsLibraryWithOptionalAppSurface(model))
            return Archetype.Library;

        // A library's sample/snippet apps (e.g. a Minimal-API demo of the library) are not the library —
        // ignore their entries and projects so they don't flip the archetype to App.
        if (!entries.IsDefaultOrEmpty && entries.Any(e =>
            AppEntryKinds.Contains(e.Kind)
            && !(e.Provenance is { } prov && ProjectClassifier.IsSamplePath(prov))))
            return Archetype.App;

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
        // W5: an exe is auxiliary only when its own project path is a sample/test/benchmark path,
        // or (for console Exes) it references a library project — not when a desktop WinExe
        // happens to reference internal library projects (e.g. Files.App → Files.Core).
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

    // F1: Framework libraries that have internal HTTP endpoints (management API, test hosts)
    // but whose primary identity is a library — not an app.
    private static readonly string[] LibraryFrameworkSignals =
    [
        ArchitectureSignals.Keys.SignalR,
        ArchitectureSignals.Keys.Grpc,
        ArchitectureSignals.Keys.MassTransit,
        ArchitectureSignals.Keys.Orleans,
        ArchitectureSignals.Keys.GraphQL,
        ArchitectureSignals.Keys.Functions,
        ArchitectureSignals.Keys.Quartz,
        ArchitectureSignals.Keys.Hangfire,
        ArchitectureSignals.Keys.Testing,
    ];

    private static bool IsLibraryWithOptionalAppSurface(DiscoveryModel model)
    {
        foreach (var sig in LibraryFrameworkSignals)
            if (model.Architecture.Has(sig))
                return true;
        return false;
    }
}
