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
        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct).ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                context.Logger.LogWarning(ex, "Failed to parse syntax tree: {Path}", filePath);
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse syntax tree: {filePath}");
                continue;
            }

            // Use shared syntax node cache — first extractor to access a file populates it
            var nodes = await context.Analysis.GetOrParseSyntaxNodesAsync(filePath, async () =>
            {
                var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
                return new FileSyntaxNodes(
                    [.. root.DescendantNodes().OfType<TypeDeclarationSyntax>()],
                    [.. root.DescendantNodes().OfType<InvocationExpressionSyntax>()]
                );
            }).ConfigureAwait(false);

            foreach (var typeDecl in nodes.TypeDeclarations)
            {
                var typeDiscovery = CreateTypeDiscovery(typeDecl, filePath);
                if (typeDiscovery == null) continue;

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
            }
        }
    }

    private static TypeDiscovery? CreateTypeDiscovery(TypeDeclarationSyntax typeDecl, string filePath)
    {
        var name = typeDecl.Identifier.ValueText;
        if (string.IsNullOrEmpty(name)) return null;

        var namespaceName = RoslynSyntaxHelpers.GetNamespace(typeDecl);
        var id = $"{namespaceName}.{name}";

        var kind = typeDecl.Kind() switch
        {
            SyntaxKind.ClassDeclaration => ModelsTypeKind.Class,
            SyntaxKind.StructDeclaration => ModelsTypeKind.Struct,
            SyntaxKind.InterfaceDeclaration => ModelsTypeKind.Interface,
            SyntaxKind.EnumDeclaration => ModelsTypeKind.Enum,
            SyntaxKind.RecordDeclaration or SyntaxKind.RecordStructDeclaration => ModelsTypeKind.Record,
            _ => ModelsTypeKind.Class,
        };

        var methods = ExtractMethods(typeDecl);
        var properties = ExtractProperties(typeDecl);
        var baseTypes = ExtractBaseTypes(typeDecl);
        var interfaces = ExtractInterfaces(typeDecl);
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
        };
    }

    private static ImmutableArray<MethodSignature> ExtractMethods(TypeDeclarationSyntax typeDecl)
    {
        var methods = new List<MethodSignature>();
        // Methods (including constructors) collected in a single pass
        foreach (var member in typeDecl.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    var paramTypes = method.ParameterList.Parameters
                        .Select(p => p.Type?.ToString() ?? "var")
                        .ToImmutableArray();
                    var paramNames = method.ParameterList.Parameters
                        .Select(p => p.Identifier.ValueText)
                        .ToImmutableArray();
                    var returnType = method.ReturnType?.ToString() ?? "void";
                    methods.Add(new MethodSignature(
                        method.Identifier.ValueText,
                        returnType,
                        paramTypes,
                        paramNames,
                        GetAccessibility(method.Modifiers),
                        method.Modifiers.Any(SyntaxKind.StaticKeyword),
                        method.Modifiers.Any(SyntaxKind.AsyncKeyword) || method.Modifiers.Any(SyntaxKind.AbstractKeyword)));
                    break;
                case ConstructorDeclarationSyntax ctor:
                    var ctorParamTypes = ctor.ParameterList.Parameters
                        .Select(p => p.Type?.ToString() ?? "var")
                        .ToImmutableArray();
                    var ctorParamNames = ctor.ParameterList.Parameters
                        .Select(p => p.Identifier.ValueText)
                        .ToImmutableArray();
                    methods.Add(new MethodSignature(
                        ctor.Identifier.ValueText,
                        "ctor",
                        ctorParamTypes,
                        ctorParamNames,
                        GetAccessibility(ctor.Modifiers),
                        ctor.Modifiers.Any(SyntaxKind.StaticKeyword),
                        false));
                    break;
            }
        }
        return methods.ToImmutableArray();
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

    private static ImmutableArray<string> ExtractBaseTypes(TypeDeclarationSyntax typeDecl)
    {
        if (typeDecl.BaseList == null) return [];

        return typeDecl.BaseList.Types
            .Select(t => (TypeName: t.Type.ToString(), Declaration: ResolveTypeDeclaration(t)))
            .Where(t => IsBaseType(t.Declaration, t.TypeName))
            .Select(t => t.TypeName)
            .ToImmutableArray();
    }

    private static ImmutableArray<string> ExtractInterfaces(TypeDeclarationSyntax typeDecl)
    {
        if (typeDecl.BaseList == null) return [];

        return typeDecl.BaseList.Types
            .Select(t => (TypeName: t.Type.ToString(), Declaration: ResolveTypeDeclaration(t)))
            .Where(t => IsInterface(t.Declaration, t.TypeName))
            .Select(t => t.TypeName)
            .ToImmutableArray();
    }

    private static bool IsBaseType(BaseTypeDeclarationSyntax? decl, string typeName)
    {
        if (decl is InterfaceDeclarationSyntax) return false;
        if (decl is not null) return true; // Class, Struct, Record — all are base types
        // Fallback: type not declared in this file — use naming convention
        return !typeName.StartsWith('I');
    }

    private static bool IsInterface(BaseTypeDeclarationSyntax? decl, string typeName)
    {
        if (decl is InterfaceDeclarationSyntax) return true;
        if (decl is not null) return false;
        // Fallback: type not declared in this file — use naming convention
        return typeName.StartsWith('I');
    }

    /// <summary>Resolves a base type syntax to its declaration by walking into namespace members or type declarations.</summary>
    private static BaseTypeDeclarationSyntax? ResolveTypeDeclaration(BaseTypeSyntax baseType)
    {
        var typeName = baseType.Type.ToString();
        // Search current file for the type declaration
        var root = baseType.SyntaxTree.GetCompilationUnitRoot();
        // Look through namespace members and top-level types for a matching declaration
        var candidate = root.DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .FirstOrDefault(t => string.Equals(t.Identifier.ValueText, typeName
, StringComparison.Ordinal) || typeName.StartsWith(t.Identifier.ValueText + "<", StringComparison.Ordinal));
        return candidate;
    }

    private static ImmutableArray<string> ExtractAttributes(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString())
            .ToImmutableArray();
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
        // 1. Base type / interface signals (highest confidence)
        if (interfaces.Any(i => i.Contains("IHostedService", StringComparison.Ordinal) || i.Contains("IEventHandler", StringComparison.Ordinal)))
            return ArchitectureLayer.Application;
        if (baseTypes.Any(b => b.Contains("DbContext", StringComparison.Ordinal)) || interfaces.Any(i => i.Contains("IRepository", StringComparison.Ordinal)))
            return ArchitectureLayer.Infrastructure;
        if (baseTypes.Any(b => b.Contains("ControllerBase", StringComparison.Ordinal) || b.Contains("Controller", StringComparison.Ordinal)))
            return ArchitectureLayer.Presentation;

        // 2. Namespace heuristics
        var lowerNs = namespaceName.ToLowerInvariant();
        if (lowerNs.Contains("presentation", StringComparison.Ordinal) || lowerNs.Contains("ui", StringComparison.Ordinal) || lowerNs.Contains("web", StringComparison.Ordinal))
            return ArchitectureLayer.Presentation;
        if (lowerNs.Contains("api", StringComparison.Ordinal) || lowerNs.Contains("controller", StringComparison.Ordinal) || lowerNs.Contains("endpoint", StringComparison.Ordinal))
            return ArchitectureLayer.Api;
        if (lowerNs.Contains("application", StringComparison.Ordinal) || lowerNs.Contains("usecase", StringComparison.Ordinal) || lowerNs.Contains("mediatr", StringComparison.Ordinal))
            return ArchitectureLayer.Application;
        if (lowerNs.Contains("domain", StringComparison.Ordinal) || lowerNs.Contains("model", StringComparison.Ordinal) || lowerNs.Contains("entity", StringComparison.Ordinal))
            return ArchitectureLayer.Domain;
        if (lowerNs.Contains("infrastructure", StringComparison.Ordinal) || lowerNs.Contains("data", StringComparison.Ordinal) || lowerNs.Contains("persistence", StringComparison.Ordinal))
            return ArchitectureLayer.Infrastructure;

        // 3. File path heuristics
        var lowerPath = filePath.ToLowerInvariant();
        if (lowerPath.Contains("\\domain\\", StringComparison.Ordinal) || lowerPath.Contains("/domain/", StringComparison.Ordinal))
            return ArchitectureLayer.Domain;
        if (lowerPath.Contains("\\application\\", StringComparison.Ordinal) || lowerPath.Contains("/application/", StringComparison.Ordinal))
            return ArchitectureLayer.Application;
        if (lowerPath.Contains("\\infrastructure\\", StringComparison.Ordinal) || lowerPath.Contains("/infrastructure/", StringComparison.Ordinal))
            return ArchitectureLayer.Infrastructure;
        if (lowerPath.Contains("\\api\\", StringComparison.Ordinal) || lowerPath.Contains("/api/", StringComparison.Ordinal) || lowerPath.Contains("\\controllers\\", StringComparison.Ordinal))
            return ArchitectureLayer.Api;
        if (lowerPath.Contains("\\presentation\\", StringComparison.Ordinal) || lowerPath.Contains("/presentation/", StringComparison.Ordinal))
            return ArchitectureLayer.Presentation;

        // 4. Naming convention heuristics
        var lowerName = typeName.ToLowerInvariant();
        if (lowerName.EndsWith("handler", StringComparison.Ordinal) || lowerName.EndsWith("service", StringComparison.Ordinal) || lowerName.EndsWith("orchestrator", StringComparison.Ordinal)
            || lowerName.EndsWith("worker", StringComparison.Ordinal) || lowerName.EndsWith("manager", StringComparison.Ordinal))
            return ArchitectureLayer.Application;
        if (lowerName.EndsWith("repository", StringComparison.Ordinal) || lowerName.EndsWith("datastore", StringComparison.Ordinal) || lowerName.EndsWith("dbcontext", StringComparison.Ordinal))
            return ArchitectureLayer.Infrastructure;
        if (lowerName.EndsWith("controller", StringComparison.Ordinal) || lowerName.EndsWith("endpoint", StringComparison.Ordinal) || lowerName.EndsWith("page", StringComparison.Ordinal))
            return ArchitectureLayer.Presentation;
        if (lowerName.EndsWith("entity", StringComparison.Ordinal) || lowerName.EndsWith("aggregate", StringComparison.Ordinal) || lowerName.EndsWith("valueobject", StringComparison.Ordinal))
            return ArchitectureLayer.Domain;

        return ArchitectureLayer.Unknown;
    }

    private static void MergePartialType(TypeDiscovery existing, TypeDiscovery other)
    {
        existing.Methods = existing.Methods.AddRange(other.Methods);
        existing.Properties = existing.Properties.AddRange(other.Properties);

        var mergedBaseTypes = existing.BaseTypes
            .Union(other.BaseTypes, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        // Can't reassign init-only property directly — use reflection as workaround
        var baseTypeProp = typeof(TypeDiscovery).GetProperty(nameof(TypeDiscovery.BaseTypes));
        if (baseTypeProp?.SetMethod != null)
            baseTypeProp.SetValue(existing, mergedBaseTypes);

        var mergedInterfaces = existing.ImplementedInterfaces
            .Union(other.ImplementedInterfaces, StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .ToImmutableArray();
        var ifaceProp = typeof(TypeDiscovery).GetProperty(nameof(TypeDiscovery.ImplementedInterfaces));
        if (ifaceProp?.SetMethod != null)
            ifaceProp.SetValue(existing, mergedInterfaces);
    }
}
