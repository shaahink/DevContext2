namespace DevContext.Core.Graph;

/// <summary>
/// What kind of codebase this is — independent of architecture <c>Style</c>. An <see cref="App"/> has
/// application entry points (HTTP/bus/hosted/scheduled); a <see cref="Library"/> is a packable component
/// with a public API and no entry points (e.g. AutoMapper). The archetype decides which renderer runs:
/// the entry-point Map vs the capability-grouped public surface (assessment G3).
/// </summary>
public enum Archetype { App, Library }

/// <summary>Decides <see cref="Archetype"/> from the entry inventory + project shape.</summary>
public static class ArchetypeDetector
{
    private static readonly EntryPointKind[] AppEntryKinds =
    [
        EntryPointKind.HttpEndpoint, EntryPointKind.MessageConsumer,
        EntryPointKind.HostedService, EntryPointKind.ScheduledJob,
    ];

    /// <summary>Library ⇔ no application entry points AND a non-executable project with a public
    /// surface exists, where any executable projects are merely auxiliary samples/benchmarks that
    /// reference the library (so AutoMapper's Benchmark/TestApp don't flip it to App). App otherwise.</summary>
    public static Archetype Detect(DiscoveryModel model, ImmutableArray<EntryPoint> entries)
    {
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
        var allExeAreAuxiliary = exe.All(e => e.ProjectReferences.Any(r =>
            libNames.Contains(Path.GetFileNameWithoutExtension(r))));
        if (!allExeAreAuxiliary)
            return Archetype.App; // a standalone executable that isn't just a sample of the library

        var packable = nonExe.Any(p => p.IsPackable);
        var hasPublicSurface = model.Types.Values.Any(t =>
            t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
            && !classifier.IsInTestProject(t.FilePath)
            && !ProjectClassifier.IsSamplePath(t.FilePath));

        return packable || hasPublicSurface ? Archetype.Library : Archetype.App;
    }
}
