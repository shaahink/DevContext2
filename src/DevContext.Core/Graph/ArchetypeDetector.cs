namespace DevContext.Core.Graph;

using DevContext.Core.Graph.EntrySurfaces;

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
    private static readonly EntryPointKind[] AppEntryKinds = EntrySurfaceCatalog.All
        .Where(d => d.Kind is not null && d.Role == SurfaceRole.AppEntry)
        .Select(d => d.Kind!.Value)
        .Distinct()
        .ToArray();

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
        // libraries only when the signal is self-sourced (ProjectName/ProjectReference).
        // Consumer apps that reference these via NuGet packages stay App.
        if (IsSelfSourcedFrameworkSignal(model))
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
