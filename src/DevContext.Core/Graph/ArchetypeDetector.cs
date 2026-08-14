namespace DevContext.Core.Graph;

using DevContext.Core.Graph.EntrySurfaces;

/// <summary>
/// What kind of codebase this is — independent of architecture <c>Style</c>. An <see cref="App"/> has
/// application entry points (HTTP/bus/hosted/scheduled); a <see cref="Library"/> is a packable component
/// with a public API and no entry points (e.g. AutoMapper); <see cref="Worker"/> is a background-service
/// app with no web endpoints; <see cref="Blazor"/> is a Blazor WASM/Server app with component pages;
/// <see cref="CliTool"/> is a command-line tool (console exes with PackAsTool/parser evidence, no web
/// surface — GitVersion). The archetype decides which renderer runs. (L7.2 Worker + Blazor; D1.1d CliTool.)
/// </summary>
public enum Archetype { App, Library, Gateway, Desktop, Worker, Blazor, CliTool }

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
        var classifier = new ProjectClassifier(model.Projects);

        // M1.9 / T1.2: a gateway is the solution's archetype only when it isn't merely one service among
        // many. A YARP/Ocelot gateway sitting in front of ≥2 real backend services is a ROLE inside a
        // microservices app (the dogfood's YarpApiGateway, eShop's BFF) → App; YARP's and Ocelot's own
        // PRODUCT repos are gateways because their only non-sample/test host IS the proxy. So count genuine
        // PEER services: real hosts (Exe or Web SDK) that are production sources — NoiseFilter excludes the
        // samples/tests/testassets/benchmarks a framework repo ships — that don't themselves reference the
        // reverse-proxy package. (Self-source is NOT the discriminator: a microservices app naming a
        // project "YarpApiGateway" self-sources the gateway signal exactly as YARP's own repo does; only
        // the peer count separates them. A library that merely references AspNetCore is not a peer host —
        // counting it flipped YARP's own ReverseProxy library into a second "service".)
        if (model.Architecture.Has(ArchitectureSignals.Keys.Gateway))
        {
            var gwNoise = new NoiseFilter(classifier);
            var peerServiceCount = model.Projects.Count(p =>
                !string.IsNullOrEmpty(p.FilePath)
                && IsRunnableHost(p)
                && gwNoise.IsProductionEntrySource(p.FilePath)
                && !p.PackageReferences.Any(pr =>
                    pr.Name.Contains("Yarp", StringComparison.OrdinalIgnoreCase)
                    || pr.Name.Contains("Ocelot", StringComparison.OrdinalIgnoreCase)));
            if (peerServiceCount >= 2)
                return Archetype.App;   // gateway is a role within a microservices app
            return Archetype.Gateway;   // the gateway IS the app — Gateway archetype
        }

        // F1: Framework libraries (SignalR, gRPC, MassTransit, Orleans, etc.) are
        // libraries only when the signal is self-sourced (ProjectName/ProjectReference).
        // T8: waived for samples-only repos — a SAMPLE named after a framework (aspire-samples'
        // Orleans voting sample) self-sources the signal without the repo being that framework.
        if (!model.SamplesAreTheProduct && IsSelfSourcedFrameworkSignal(model))
            return Archetype.Library;

        // G9.1: the library shape is computed ONCE, HERE, and consulted twice — by the rung below and,
        // unchanged, as the bottom rung. It used to be computed only at the bottom, which is the whole
        // defect: the ladder decided CliTool (and, for a demo app, App/Desktop) before the
        // auxiliary-executable test it already owned was ever asked. Measured on dotnet/command-line-api:
        // src/System.CommandLine.Suggest is a real dotnet tool (OutputType Exe + PackAsTool) under a
        // production path, so the CliTool rung fired and returned before the bottom rung could observe
        // that the exe merely CONSUMES System.CommandLine — the packable library that is the product.
        var shape = DescribeLibraryShape(model, classifier);
        if (!model.SamplesAreTheProduct && shape.IsPackableLibraryWithOnlyAuxiliaryExes)
            return Archetype.Library;

        // A3/B4 (Prism D1.1d): CliTool — the product is a command-line tool. Fires when production
        // code has console executables with explicit TOOL evidence (PackAsTool/ToolCommandName in the
        // csproj, or a CLI-parser package) and NO web/desktop surface anywhere (a microservices repo
        // shipping a migrator utility stays App; a desktop app stays Desktop). Checked before the
        // entries rung so parser-based command entries route here, not to the generic App map.
        {
            var prodConsoleExes = model.Projects
                .Where(p => p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true
                    && p.OutputType?.Contains("WinExe", StringComparison.OrdinalIgnoreCase) != true)
                .Where(p => !classifier.IsInTestProject(p.FilePath)
                    && classifier.IsProduction(p, model.SamplesAreTheProduct)
                    && !p.HasSdk(SdkIds.Web))
                .ToList();
            var hasWebOrDesktopSurface =
                model.Architecture.Has(ArchitectureSignals.Keys.Controllers)
                || model.Architecture.Has(ArchitectureSignals.Keys.MinimalApis)
                || model.Architecture.Has(ArchitectureSignals.Keys.FastEndpoints)
                || model.Architecture.Has(ArchitectureSignals.Keys.RazorPages)
                || model.Architecture.Has(ArchitectureSignals.Keys.Blazor)
                || model.Architecture.Has(ArchitectureSignals.Keys.DesktopUi)
                || model.Architecture.Has(ArchitectureSignals.Keys.Maui)
                || model.Architecture.Has(ArchitectureSignals.Keys.SignalR)
                || model.Architecture.Has(ArchitectureSignals.Keys.Grpc);
            if (prodConsoleExes.Count > 0
                && !hasWebOrDesktopSurface
                && prodConsoleExes.Any(IsCliToolCandidate))
                return Archetype.CliTool;
        }

        // A library's sample/snippet apps (e.g. a Minimal-API demo of the library) are not the library —
        // ignore their entries and projects so they don't flip the archetype to App. T8: unless the
        // samples ARE the product (samples-only repo) — then their entries are the app evidence.
        if (!entries.IsDefaultOrEmpty && entries.Any(e =>
            AppEntryKinds.Contains(e.Kind)
            && (model.SamplesAreTheProduct
                || !(e.Provenance is { } prov && ProjectClassifier.IsSamplePath(prov)))))
            return DetectAppSubtype(model, entries);

        if (shape.NoProductionProjects)
            return Archetype.App;
        if (shape.NoLibraryProjects)
            return Archetype.App; // pure executable(s)
        if (!shape.AllExeAreAuxiliary)
            return Archetype.App; // a standalone executable that isn't just a sample of the library

        return shape.Packable || shape.HasPublicSurface ? Archetype.Library : Archetype.App;
    }

    /// <summary>G9.1 — "is this repo a library whose executables merely demonstrate or consume it?", as a
    /// value so the ladder can ask it more than once. Every field is the bottom rung's original
    /// computation, moved verbatim; nothing here is new evidence.</summary>
    private readonly record struct LibraryShape(
        bool NoProductionProjects,
        bool NoLibraryProjects,
        bool HasExecutables,
        bool AllExeAreAuxiliary,
        bool Packable,
        bool HasPublicSurface)
    {
        /// <summary>G9.1 — the narrow, high-evidence form of the shape, and the only one allowed to
        /// overrule a rung above the bottom. Three guards make it safe to promote:
        /// <list type="bullet">
        /// <item><see cref="HasExecutables"/> — there must actually BE an executable about to decide the
        /// archetype. A Web-SDK host declares no <c>OutputType</c>, so every ordinary web app has an empty
        /// exe set and <c>All()</c> over it is vacuously true; without this guard such a repo could reach
        /// Library the moment any of its projects declared packability.</item>
        /// <item><see cref="Packable"/> means an EXPLICIT <c>IsPackable</c>/<c>GeneratePackageOnBuild</c>
        /// declaration (see <c>CsprojReader.ResolveIsPackable</c>) — the symmetric counterpart of the
        /// <c>PackAsTool</c> that makes the exe look like the product. A public surface is deliberately NOT
        /// enough: every library has public types, so that bar would flip GitVersion (whose GitVersion.Core
        /// declares no packability) out of CliTool. Measured across the eval poles: of the repos whose truth
        /// is App or Desktop, not one declares packability anywhere.</item>
        /// <item>the call site adds <c>!model.SamplesAreTheProduct</c> — in a samples-only repo the samples
        /// ARE the product, so no executable in it is auxiliary to anything.</item>
        /// </list></summary>
        public bool IsPackableLibraryWithOnlyAuxiliaryExes
            => !NoProductionProjects && !NoLibraryProjects && HasExecutables && AllExeAreAuxiliary && Packable;
    }

    /// <summary>D1.3 (#20) — "are this repo's executables merely auxiliary to a library it ships?",
    /// the verdict the rung at the top of <see cref="Detect"/> already makes, exposed so the SERVICE
    /// population can obey it instead of re-deciding "what is a service" for itself.
    /// <para>AutoMapper's Atlas read "1 services (1 drawn)" and named <c>TestApp</c>: the archetype
    /// detector had already judged that exe auxiliary (that judgement is WHY the repo reads Library),
    /// and <see cref="Graph2.ServiceBoundaryInference"/> never asked. Two judgements about the same
    /// projects, and the page showed both.</para>
    /// <para>This is deliberately the COMPOSITE, repo-level verdict, never the per-project half:
    /// "a non-WinExe exe that transitively references a library" is true of every eShop service, so a
    /// per-project reading of the same evidence would empty a real microservices canvas.</para></summary>
    public static bool ExecutablesAreAuxiliaryToALibrary(DiscoveryModel model)
        => !model.SamplesAreTheProduct
            && DescribeLibraryShape(model, new ProjectClassifier(model.Projects))
                .IsPackableLibraryWithOnlyAuxiliaryExes;

    private static LibraryShape DescribeLibraryShape(DiscoveryModel model, ProjectClassifier classifier)
    {
        // D1.1b: holder csproj (NoTargets/Traversal SDKs) and build-tooling exes (Cake/Nuke/Bullseye)
        // are not archetype evidence — SE.Redis's root Traversal Build.csproj is an "Exe" that
        // references no library and blocked the Library verdict.
        var nonTest = model.Projects
            .Where(p => !classifier.IsInTestProject(p.FilePath))
            .Where(p => !ProjectClassifier.IsHolderProject(p) && !classifier.IsBuildTooling(p))
            .Where(p => model.SamplesAreTheProduct || !ProjectClassifier.IsSamplePath(p.FilePath))
            .ToList();
        if (nonTest.Count == 0)
            return new LibraryShape(NoProductionProjects: true, false, false, false, false, false);

        static bool IsExe(ProjectInfo p) => p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        var exe = nonTest.Where(IsExe).ToList();
        var nonExe = nonTest.Where(p => !IsExe(p)).ToList();
        if (nonExe.Count == 0)
            return new LibraryShape(false, NoLibraryProjects: true, false, false, false, false);

        var libNames = nonExe.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // A1 (Prism D1.1a): the auxiliary-exe reference may be TRANSITIVE — Newtonsoft.Json.TestConsole
        // references only Newtonsoft.Json.Tests, which references the library. Walk the in-solution
        // project-reference graph (through test projects too) from the exe to any library project.
        var projectsByName = model.Projects
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        bool ReferencesLibraryTransitively(ProjectInfo start)
        {
            var stack = new Stack<string>(start.ProjectReferences.Select(r => Path.GetFileNameWithoutExtension(r)));
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (stack.Count > 0)
            {
                var name = stack.Pop();
                if (!seen.Add(name)) continue;
                if (libNames.Contains(name)) return true;
                if (projectsByName.TryGetValue(name, out var via))
                    foreach (var r in via.ProjectReferences)
                        stack.Push(Path.GetFileNameWithoutExtension(r));
            }
            return false;
        }

        var allExeAreAuxiliary = exe.All(e =>
            !model.SamplesAreTheProduct && ProjectClassifier.IsSamplePath(e.FilePath)
            || ProjectClassifier.IsTestPath(e.FilePath)
            || e.OutputType?.Contains("WinExe", StringComparison.OrdinalIgnoreCase) != true
                && ReferencesLibraryTransitively(e));

        var packable = nonExe.Any(p => p.IsPackable);
        var hasPublicSurface = model.OrderedTypes.Any(t =>
            t.Accessibility == Microsoft.CodeAnalysis.Accessibility.Public
            && !classifier.IsInTestProject(t.FilePath)
            && (model.SamplesAreTheProduct || !ProjectClassifier.IsSamplePath(t.FilePath)));

        return new LibraryShape(
            NoProductionProjects: false,
            NoLibraryProjects: false,
            HasExecutables: exe.Count > 0,
            AllExeAreAuxiliary: allExeAreAuxiliary,
            Packable: packable,
            HasPublicSurface: hasPublicSurface);
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
            && (model.SamplesAreTheProduct
                || !(e.Provenance is { } prov && ProjectClassifier.IsSamplePath(prov))))
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

    // D1.1d: CLI argument-parser frameworks — referencing one from a console exe is tool evidence.
    private static readonly string[] CliParserPackages =
        ["Spectre.Console.Cli", "System.CommandLine", "McMaster.Extensions.CommandLineUtils",
         "CommandLineParser", "Cocona", "ConsoleAppFramework"];

    /// <summary>D1.1d — true when the project carries explicit CLI-tool evidence: it packs as a dotnet
    /// tool (<c>PackAsTool</c>/<c>ToolCommandName</c>) or references a CLI argument-parser framework.
    /// Shared by the archetype rung and the plain-Main entry fallback (CliCommandExtractor).</summary>
    public static bool IsCliToolCandidate(ProjectInfo p)
        => p.IsToolPackaged
            || p.PackageReferences.Any(pr =>
                CliParserPackages.Any(m => pr.Name.StartsWith(m, StringComparison.OrdinalIgnoreCase)));

    /// <summary>T1.2 — true when the project is a genuinely runnable host: an executable (OutputType Exe)
    /// or a Web SDK project (Microsoft.NET.Sdk.Web). Deliberately does NOT treat "references an AspNetCore
    /// package" as a service — a library built on ASP.NET Core (e.g. YARP's own ReverseProxy) is not a
    /// peer service, and counting it flipped the gateway archetype from Gateway to App (M1.9 regression).</summary>
    private static bool IsRunnableHost(ProjectInfo p)
    {
        var isExe = p.OutputType?.Contains("Exe", StringComparison.OrdinalIgnoreCase) == true;
        return isExe || p.HasSdk(SdkIds.Web);
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
