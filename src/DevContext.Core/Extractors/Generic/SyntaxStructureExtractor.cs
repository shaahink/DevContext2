using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using ModelsTypeKind = DevContext.Core.Models.TypeKind;

namespace DevContext.Core.Extractors.Generic;

/// <summary>Walks syntax trees of all .cs files to discover type declarations and their members.</summary>
[ExtractorOrder(20)]
public sealed class SyntaxStructureExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "SyntaxStructureExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Generic;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage2Parallel;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [], [ArchitectureSignals.Keys.Controllers],
        ["model.Types"],
        "Walks syntax trees of all .cs files to discover type declarations and emits controller signal fallback");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // Two-phase, output-preserving parallelism (P5): parse + build per-file TypeDiscovery lists in
        // parallel (the parse/walk is the Stage-2 floor), then COMMIT to the shared model single-threaded
        // in deterministic file order. The shared collections are concurrent, but the partial-type merge
        // (read-modify-write) and the member/signal ORDER are not — so committing serially in source
        // order makes the output byte-identical to the previous serial loop while the heavy work fans out.
        var files = context.Analysis.AllSourceFiles;
        var perFile = new List<TypeDiscovery>[files.Count];

        // A repo that IS the SignalR framework self-sources the signal via ProjectName (0.7f);
        // a higher-confidence SyntaxPattern registration would overwrite DetectedVia and break
        // Library archetype detection (ArchetypeDetector.IsSelfSourcedFrameworkSignal).
        var signalRSelfPatterns = Graph.EntrySurfaces.EntrySurfaceCatalog.All
            .First(d => d.SignalKey == ArchitectureSignals.Keys.SignalR).SelfNamePatterns;
        var selfSourcesSignalR = model.Projects.Any(p =>
            signalRSelfPatterns.Any(pat => p.Name.Contains(pat, StringComparison.OrdinalIgnoreCase)));

        // B1 (Prism D1.2a): MapHub<T> is signalr evidence too — package-free apps map their hubs in
        // Program.cs/Startup.cs even when the Hub subclass lives in a project outside the scan scope.
        var fileMapsHub = new bool[files.Count];

        var opts = new ParallelOptions { CancellationToken = ct, MaxDegreeOfParallelism = Environment.ProcessorCount };
        await Parallel.ForEachAsync(Enumerable.Range(0, files.Count), opts, async (i, innerCt) =>
        {
            var filePath = files[i];

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, innerCt);
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning(ex, "Failed to parse syntax tree: {Path}", filePath);
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse syntax tree: {filePath}");
                return;
            }

            // Use shared syntax node cache — first extractor to access a file populates it (thread-safe Lazy).
            var nodes = await context.Analysis.GetOrParseSyntaxNodesAsync(filePath, async () =>
            {
                var root = await syntaxTree.GetRootAsync(innerCt).ConfigureAwait(false);
                return new FileSyntaxNodes(
                    [.. root.DescendantNodes().OfType<TypeDeclarationSyntax>()],
                    [.. root.DescendantNodes().OfType<InvocationExpressionSyntax>()]
                );
            });

            // G8.2 — build the base-type index ONCE per file. It used to be a full tree walk per
            // base-list entry (see ResolveTypeDeclaration), which is quadratic INSIDE one file:
            // HotChocolate's 11 MB generated GraphQL client spent 20 min 17 s of a 21 min 15 s
            // analysis right here. Empty index when the file declares no types — nothing to resolve.
            var declIndex = nodes.TypeDeclarations.Length > 0
                ? BuildDeclarationIndex(await syntaxTree.GetRootAsync(innerCt).ConfigureAwait(false))
                : EmptyDeclarationIndex;

            var list = new List<TypeDiscovery>(nodes.TypeDeclarations.Length);
            foreach (var typeDecl in nodes.TypeDeclarations)
            {
                var typeDiscovery = CreateTypeDiscovery(typeDecl, filePath, declIndex);
                if (typeDiscovery != null) list.Add(typeDiscovery);
            }
            perFile[i] = list;

            fileMapsHub[i] = nodes.Invocations.Any(inv =>
                inv.Expression.DescendantNodesAndSelf()
                    .OfType<GenericNameSyntax>()
                    .Any(g => g.Identifier.ValueText == "MapHub"));
        });

        // Phase 2: commit in source-file order (identical ordering to the prior serial loop).
        foreach (var list in perFile)
        {
            if (list is null) continue;
            foreach (var typeDiscovery in list)
            {
                if (!model.Types.TryAdd(typeDiscovery.Id, typeDiscovery))
                {
                    // Merge partial class fields and methods
                    if (model.Types.TryGetValue(typeDiscovery.Id, out var existing))
                        MergePartialType(existing, typeDiscovery);
                    continue;
                }

                // Signal fallback: detect controllers from base type inheritance and EF Core DbContext
                if (typeDiscovery.BaseTypes.Any(b =>
                    b is "ControllerBase" or "Controller" or "ApiController"
                    || b.StartsWith("Controller<", StringComparison.Ordinal)))
                {
                    model.Architecture.Register(FeatureSignal.CreateDetected(
                        ArchitectureSignals.Keys.Controllers, 0.9f, "SyntaxPattern",
                        $"Class {typeDiscovery.Name} derives from {string.Join(", ", typeDiscovery.BaseTypes)}"));
                }

                // Razor Pages: detect PageModel base type
                if (typeDiscovery.BaseTypes.Any(b => b == "PageModel"))
                {
                    model.Architecture.Register(FeatureSignal.CreateDetected(
                        ArchitectureSignals.Keys.RazorPages, 0.9f, "SyntaxPattern",
                        $"Class {typeDiscovery.Name} derives from PageModel"));
                }

                // SignalR: detect Hub base type. Built-in SignalR ships in the ASP.NET Core
                // shared framework — modern apps carry no SignalR package reference at all.
                // B1 (Prism D1.2a): also match the namespace-qualified base as written
                // (": Microsoft.AspNetCore.SignalR.Hub" — dotnet-podcasts AND bitwarden).
                if (!selfSourcesSignalR && typeDiscovery.BaseTypes.Any(
                    Specific.SignalRHubExtractor.IsHubBaseName))
                {
                    model.Architecture.Register(FeatureSignal.CreateDetected(
                        ArchitectureSignals.Keys.SignalR, 0.9f, "SyntaxPattern",
                        $"Class {typeDiscovery.Name} derives from Hub"));
                }
            }
        }

        // B1 (Prism D1.2a): MapHub<T> route mapping fires the signal package-free.
        if (!selfSourcesSignalR)
        {
            for (var i = 0; i < files.Count; i++)
            {
                if (!fileMapsHub[i]) continue;
                model.Architecture.Register(FeatureSignal.CreateDetected(
                    ArchitectureSignals.Keys.SignalR, 0.9f, "SyntaxPattern",
                    $"MapHub<T> mapped in {Path.GetFileName(files[i])}"));
                break;
            }
        }

        // C1 (Prism D2): Blazor components become real types via their @code virtual trees — the
        // component class, its @inject properties, and its @code methods, in the component's true
        // namespace so a .razor.cs code-behind partial MERGES rather than duplicating. Markup is
        // never parsed (RazorCodeVirtualizer extracts the @code block text only). Committed after
        // the .cs loop in AllContentFiles order — deterministic, and the code-behind (a .cs file)
        // wins the TryAdd so merge semantics match any other partial type. No signal registration:
        // blazor/controller signals have their own detectors.
        await foreach (var (razorPath, tree) in Utilities.RazorCodeVirtualizer.EnumerateVirtualTreesAsync(context, ct))
        {
            var root = await tree.GetRootAsync(ct).ConfigureAwait(false);
            var razorIndex = BuildDeclarationIndex(root);
            foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var typeDiscovery = CreateTypeDiscovery(typeDecl, razorPath, razorIndex);
                if (typeDiscovery is null) continue;
                if (!model.Types.TryAdd(typeDiscovery.Id, typeDiscovery)
                    && model.Types.TryGetValue(typeDiscovery.Id, out var existing))
                    MergePartialType(existing, typeDiscovery);
            }
        }
    }

    private static TypeDiscovery? CreateTypeDiscovery(TypeDeclarationSyntax typeDecl, string filePath,
        IReadOnlyDictionary<string, BaseTypeDeclarationSyntax> declIndex)
    {
        var name = typeDecl.Identifier.ValueText;
        if (string.IsNullOrEmpty(name)) return null;

        var namespaceDecl = typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        var namespaceName = namespaceDecl?.Name.ToString() ?? "global";
        // Batch A: structural identity — nested-type chain + generic arity live in the Id
        // (Ns.Outer.Inner`2); Name stays the bare identifier for short-name joins and display.
        var id = Graph2.SymbolCanon.ForTypeDecl(typeDecl);

        var kind = typeDecl.Kind() switch
        {
            SyntaxKind.ClassDeclaration => ModelsTypeKind.Class,
            SyntaxKind.StructDeclaration => ModelsTypeKind.Struct,
            SyntaxKind.InterfaceDeclaration => ModelsTypeKind.Interface,
            SyntaxKind.EnumDeclaration => ModelsTypeKind.Enum,
            SyntaxKind.RecordDeclaration or SyntaxKind.RecordStructDeclaration => ModelsTypeKind.Record,
            _ => ModelsTypeKind.Class,
        };

        var methods = ExtractMethods(typeDecl).Concat(ExtractConstructors(typeDecl)).ToImmutableArray();
        var properties = ExtractProperties(typeDecl);
        var baseTypes = ExtractBaseTypes(typeDecl, declIndex);
        var interfaces = ExtractInterfaces(typeDecl, declIndex);
        var attributes = ExtractAttributes(typeDecl);

        return new TypeDiscovery
        {
            Id = id,
            Name = name,
            Namespace = namespaceName,
            FilePath = filePath,
            Kind = kind,
            Accessibility = typeDecl switch
            {
                ClassDeclarationSyntax c => GetAccessibility(c.Modifiers),
                StructDeclarationSyntax s => GetAccessibility(s.Modifiers),
                InterfaceDeclarationSyntax i => GetAccessibility(i.Modifiers),
                RecordDeclarationSyntax r => GetAccessibility(r.Modifiers),
                _ => Accessibility.Public,
            },
            Layer = InferLayer(namespaceName, filePath, name, baseTypes, interfaces),
            Methods = methods,
            Properties = properties,
            BaseTypes = baseTypes,
            ImplementedInterfaces = interfaces,
            Attributes = attributes,
            XmlDoc = ExtractXmlDocSummary(typeDecl),
        };
    }

    private static ImmutableArray<MethodSignature> ExtractMethods(TypeDeclarationSyntax typeDecl)
    {
        var methods = new List<MethodSignature>();
        foreach (var method in typeDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            var paramTypes = method.ParameterList.Parameters
                .Select(p => p.Type?.ToString() ?? "var")
                .ToImmutableArray();
            var paramNames = method.ParameterList.Parameters
                .Select(p => p.Identifier.ValueText)
                .ToImmutableArray();
            var returnType = method.ReturnType?.ToString() ?? "void";

            var firstParam = method.ParameterList.Parameters.FirstOrDefault();
            var isExtension = method.Modifiers.Any(SyntaxKind.StaticKeyword)
                && firstParam is not null
                && firstParam.Modifiers.Any(SyntaxKind.ThisKeyword);

            methods.Add(new MethodSignature(
                method.Identifier.ValueText,
                returnType,
                paramTypes,
                paramNames,
                GetAccessibility(method.Modifiers),
                method.Modifiers.Any(SyntaxKind.StaticKeyword),
                method.Modifiers.Any(SyntaxKind.AsyncKeyword) || method.Modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                IsExtension = isExtension,
                ExtendedType = isExtension ? firstParam!.Type?.ToString() : null,
                XmlDoc = ExtractXmlDocSummary(method),
            });
        }

        return methods.ToImmutableArray();
    }

    private static ImmutableArray<MethodSignature> ExtractConstructors(TypeDeclarationSyntax typeDecl)
    {
        var ctors = new List<MethodSignature>();
        foreach (var ctor in typeDecl.Members.OfType<ConstructorDeclarationSyntax>())
        {
            var paramTypes = ctor.ParameterList.Parameters
                .Select(p => p.Type?.ToString() ?? "var")
                .ToImmutableArray();
            var paramNames = ctor.ParameterList.Parameters
                .Select(p => p.Identifier.ValueText)
                .ToImmutableArray();

            ctors.Add(new MethodSignature(
                ctor.Identifier.ValueText,
                "ctor",
                paramTypes,
                paramNames,
                GetAccessibility(ctor.Modifiers),
                ctor.Modifiers.Any(SyntaxKind.StaticKeyword),
                false));
        }

        return ctors.ToImmutableArray();
    }

    private static ImmutableArray<PropertySignature> ExtractProperties(TypeDeclarationSyntax typeDecl)
    {
        var properties = new List<PropertySignature>();
        foreach (var prop in typeDecl.Members.OfType<PropertyDeclarationSyntax>())
        {
            properties.Add(new PropertySignature(
                prop.Identifier.ValueText,
                prop.Type?.ToString() ?? "object",
                GetAccessibility(prop.Modifiers),
                prop.Modifiers.Any(SyntaxKind.StaticKeyword),
                prop.Initializer != null || prop.AccessorList?.Accessors.All(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)) == true,
                prop.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)) ?? true,
                prop.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.SetKeyword) || a.Keyword.IsKind(SyntaxKind.InitKeyword)) ?? false));
        }

        return properties.ToImmutableArray();
    }

    private static ImmutableArray<string> ExtractBaseTypes(TypeDeclarationSyntax typeDecl,
        IReadOnlyDictionary<string, BaseTypeDeclarationSyntax> declIndex)
    {
        if (typeDecl.BaseList == null) return [];

        return typeDecl.BaseList.Types
            .Select(t => (TypeName: t.Type.ToString(), Declaration: ResolveTypeDeclaration(t, declIndex)))
            .Where(t => IsBaseType(t.Declaration, t.TypeName))
            .Select(t => t.TypeName)
            .ToImmutableArray();
    }

    private static ImmutableArray<string> ExtractInterfaces(TypeDeclarationSyntax typeDecl,
        IReadOnlyDictionary<string, BaseTypeDeclarationSyntax> declIndex)
    {
        if (typeDecl.BaseList == null) return [];

        return typeDecl.BaseList.Types
            .Select(t => (TypeName: t.Type.ToString(), Declaration: ResolveTypeDeclaration(t, declIndex)))
            .Where(t => IsInterface(t.Declaration, t.TypeName))
            .Select(t => t.TypeName)
            .ToImmutableArray();
    }

    private static bool IsBaseType(BaseTypeDeclarationSyntax? decl, string typeName)
    {
        if (decl is InterfaceDeclarationSyntax) return false;
        if (decl is not null) return true; // Class, Struct, Record — all are base types
        // Fallback: type not declared in this file — use naming convention
        return !typeName.StartsWith("I");
    }

    private static bool IsInterface(BaseTypeDeclarationSyntax? decl, string typeName)
    {
        if (decl is InterfaceDeclarationSyntax) return true;
        if (decl is not null) return false;
        // Fallback: type not declared in this file — use naming convention
        return typeName.StartsWith("I");
    }

    private static readonly Dictionary<string, BaseTypeDeclarationSyntax> EmptyDeclarationIndex =
        new(StringComparer.Ordinal);

    /// <summary>G8.2 — every type declared in one file, keyed by its bare identifier, FIRST in
    /// document order. Built once per file; see <see cref="ResolveTypeDeclaration"/> for why the
    /// first-in-document-order rule is the one that has to be preserved.</summary>
    private static Dictionary<string, BaseTypeDeclarationSyntax> BuildDeclarationIndex(SyntaxNode root)
    {
        var index = new Dictionary<string, BaseTypeDeclarationSyntax>(StringComparer.Ordinal);
        foreach (var decl in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            index.TryAdd(decl.Identifier.ValueText, decl);
        return index;
    }

    /// <summary>Resolves a base type syntax to a declaration in the SAME file, or null when the type
    /// is declared elsewhere (the callers then fall back to the naming convention).
    ///
    /// G8.2 — this used to be
    /// <c>root.DescendantNodes().OfType&lt;BaseTypeDeclarationSyntax&gt;().FirstOrDefault(t =&gt;
    /// t.Identifier.ValueText == typeName || typeName.StartsWith(t.Identifier.ValueText + "&lt;"))</c>,
    /// i.e. a full walk of the whole file per base-list entry — quadratic inside one file, and the
    /// R1 scale wall (HotChocolate: 20 min 17 s of a 21 min 15 s analysis, all of it here).
    ///
    /// The index lookup below is EQUIVALENT, not merely close. A declaration's
    /// <c>Identifier.ValueText</c> is a single token and can never contain <c>&lt;</c>, so the old
    /// predicate's two branches are mutually exclusive: when <c>typeName</c> contains <c>&lt;</c>
    /// only the StartsWith branch can fire, and it fires exactly when the identifier equals
    /// <c>typeName</c> truncated at the first <c>&lt;</c>; when it does not, only the equality branch
    /// can fire. So "first node in document order satisfying either" is exactly "the first-declared
    /// type whose identifier equals that truncated key" — which is what
    /// <see cref="BuildDeclarationIndex"/> stores.</summary>
    private static BaseTypeDeclarationSyntax? ResolveTypeDeclaration(BaseTypeSyntax baseType,
        IReadOnlyDictionary<string, BaseTypeDeclarationSyntax> declIndex)
    {
        var typeName = baseType.Type.ToString();
        var generic = typeName.IndexOf('<');
        var key = generic >= 0 ? typeName[..generic] : typeName;
        return declIndex.TryGetValue(key, out var candidate) ? candidate : null;
    }

    private static ImmutableArray<string> ExtractAttributes(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString())
            .ToImmutableArray();
    }

    /// <summary>Extracts the <c>&lt;summary&gt;</c> prose from a node's leading XML doc comment — build-free,
    /// syntactic (no SemanticModel). Walks nested elements (e.g. <c>&lt;para&gt;</c>) and returns the
    /// whitespace-collapsed text, or null when there is no doc comment or summary.</summary>
    private static string? ExtractXmlDocSummary(SyntaxNode node)
    {
        var doc = node.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();
        if (doc is null) return null;

        var summary = doc.Content
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(e => e.StartTag.Name.LocalName.ValueText == "summary");
        if (summary is null) return null;

        var raw = string.Concat(summary.DescendantNodes()
            .OfType<XmlTextSyntax>()
            .SelectMany(t => t.TextTokens)
            .Where(tok => tok.IsKind(SyntaxKind.XmlTextLiteralToken) || tok.IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
            .Select(tok => tok.ValueText));

        var collapsed = string.Join(" ", raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrEmpty(collapsed) ? null : collapsed;
    }

    private static Accessibility GetAccessibility(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(SyntaxKind.PublicKeyword)) return Accessibility.Public;
        if (modifiers.Any(SyntaxKind.InternalKeyword)) return Accessibility.Internal;
        if (modifiers.Any(SyntaxKind.ProtectedKeyword)) return Accessibility.Protected;
        if (modifiers.Any(SyntaxKind.PrivateKeyword)) return Accessibility.Private;
        return Accessibility.Public;
    }

    private static ArchitectureLayer InferLayer(string namespaceName, string filePath,
        string typeName, ImmutableArray<string> baseTypes, ImmutableArray<string> interfaces)
    {
        var lowerName = typeName.ToLowerInvariant();

        // 0. High-confidence naming signals (before namespace/filesystem heuristics)
        if (lowerName.StartsWith("add"))
            return ArchitectureLayer.Infrastructure;

        // 1. Base type / interface signals (highest confidence)
        if (interfaces.Any(i => i.Contains("IHostedService") || i.Contains("IEventHandler") || i.Contains("IConsumer")))
            return ArchitectureLayer.Application;
        if (baseTypes.Any(b => b.Contains("DbContext")) || interfaces.Any(i => i.Contains("IRepository")))
            return ArchitectureLayer.Infrastructure;
        if (baseTypes.Any(b => b.Contains("ControllerBase") || b.Contains("Controller")))
            return ArchitectureLayer.Presentation;

        // 2. Namespace heuristics (ordered most-specific to least)
        var lowerNs = namespaceName.ToLowerInvariant();
        if (lowerNs.Contains("dependencyinjection") || lowerNs.Contains("extensions"))
            return ArchitectureLayer.Infrastructure;
        if (lowerNs.Contains("presentation") || lowerNs.Contains("ui") || lowerNs.Contains("web"))
            return ArchitectureLayer.Presentation;
        if (lowerNs.Contains("api") || lowerNs.Contains("controller") || lowerNs.Contains("endpoint"))
            return ArchitectureLayer.Api;
        if (lowerNs.Contains("application") || lowerNs.Contains("usecase") || lowerNs.Contains("mediatr"))
            return ArchitectureLayer.Application;
        if (lowerNs.Contains("domain") || lowerNs.Contains("model") || lowerNs.Contains("entity"))
            return ArchitectureLayer.Domain;
        if (lowerNs.Contains("infrastructure") || lowerNs.Contains("data") || lowerNs.Contains("persistence")
            || lowerNs.Contains("grpc") || lowerNs.Contains("client") || lowerNs.Contains("proxy")
            || lowerNs.Contains("di"))
            return ArchitectureLayer.Infrastructure;
        if (lowerNs.Contains("contracts") || lowerNs.Contains("dto") || lowerNs.Contains("messages"))
            return ArchitectureLayer.Contracts;
        if (lowerNs.Contains("shared") || lowerNs.Contains("common"))
            return ArchitectureLayer.Shared;
        if (lowerNs.Contains("buildingblocks") || lowerNs.Contains("building-blocks"))
            return ArchitectureLayer.Shared;
        if (lowerNs.Contains("eventbus") || lowerNs.Contains("messaging") || lowerNs.Contains("bus"))
            return ArchitectureLayer.Infrastructure;
        if (lowerNs.Contains("middleware") || lowerNs.Contains("filter") || lowerNs.Contains("interceptor"))
            return ArchitectureLayer.Infrastructure;
        if (lowerNs.Contains("config") || lowerNs.Contains("configuration") || lowerNs.Contains("options"))
            return ArchitectureLayer.Infrastructure;

        // 3. File path heuristics
        var lowerPath = filePath.ToLowerInvariant();
        if (lowerPath.Contains("\\domain\\") || lowerPath.Contains("/domain/"))
            return ArchitectureLayer.Domain;
        if (lowerPath.Contains("\\application\\") || lowerPath.Contains("/application/"))
            return ArchitectureLayer.Application;
        if (lowerPath.Contains("\\infrastructure\\") || lowerPath.Contains("/infrastructure/"))
            return ArchitectureLayer.Infrastructure;
        if (lowerPath.Contains("\\api\\") || lowerPath.Contains("/api/") || lowerPath.Contains("\\controllers\\"))
            return ArchitectureLayer.Api;
        if (lowerPath.Contains("\\presentation\\") || lowerPath.Contains("/presentation/"))
            return ArchitectureLayer.Presentation;
        if (lowerPath.Contains("\\contracts\\") || lowerPath.Contains("/contracts/") || lowerPath.Contains("\\dto\\"))
            return ArchitectureLayer.Contracts;
        if (lowerPath.Contains("\\shared\\") || lowerPath.Contains("/shared/") || lowerPath.Contains("\\common\\"))
            return ArchitectureLayer.Shared;

        // 4. Naming convention heuristics
        if (lowerName.EndsWith("handler") || lowerName.EndsWith("service") || lowerName.EndsWith("orchestrator")
            || lowerName.EndsWith("worker") || lowerName.EndsWith("manager") || lowerName.EndsWith("consumer")
            || lowerName.EndsWith("behavior"))
            return ArchitectureLayer.Application;
        if (lowerName.EndsWith("repository") || lowerName.EndsWith("datastore") || lowerName.EndsWith("dbcontext"))
            return ArchitectureLayer.Infrastructure;
        if (lowerName.EndsWith("controller") || lowerName.EndsWith("endpoint") || lowerName.EndsWith("page"))
            return ArchitectureLayer.Presentation;
        if (lowerName.EndsWith("entity") || lowerName.EndsWith("aggregate") || lowerName.EndsWith("valueobject"))
            return ArchitectureLayer.Domain;
        if (lowerName.EndsWith("dto") || lowerName.EndsWith("request") || lowerName.EndsWith("response")
            || lowerName.EndsWith("command") || lowerName.EndsWith("query"))
            return ArchitectureLayer.Contracts;
        if (lowerName.EndsWith("extensions") || lowerName.EndsWith("filter") || lowerName.EndsWith("middleware")
            || lowerName.EndsWith("interceptor")
            || lowerName.Contains("profile") || lowerName.EndsWith("mapping") || lowerName.EndsWith("mapper")
            || lowerName.EndsWith("attribute")
            || lowerName.EndsWith("options") || lowerName.EndsWith("settings")
            || lowerName.EndsWith("healthcheck"))
            return ArchitectureLayer.Infrastructure;
        if (lowerName.EndsWith("event"))
            return ArchitectureLayer.Domain;
        if (lowerName.EndsWith("exception") || lowerName.EndsWith("result"))
            return ArchitectureLayer.Shared;

        return ArchitectureLayer.Unknown;
    }

    private static void MergePartialType(TypeDiscovery existing, TypeDiscovery other)
    {
        existing.Methods = existing.Methods.AddRange(other.Methods);
        existing.Properties = existing.Properties.AddRange(other.Properties);

        existing.BaseTypes = existing.BaseTypes
            .Union(other.BaseTypes)
            .Distinct()
            .ToImmutableArray();

        existing.ImplementedInterfaces = existing.ImplementedInterfaces
            .Union(other.ImplementedInterfaces)
            .Distinct()
            .ToImmutableArray();
    }
}
