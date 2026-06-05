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
        [], [],
        ["model.Types"],
        "Walks syntax trees of all .cs files to discover type declarations");
    /// <summary>Determines whether this extractor should run.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel) => true;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        await foreach (var filePath in EnumerateSourceFilesAsync(context, ct))
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse syntax tree: {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .ToList();

            foreach (var typeDecl in typeDeclarations)
            {
                var typeDiscovery = CreateTypeDiscovery(typeDecl, filePath);
                if (typeDiscovery == null) continue;

                if (!model.Types.TryAdd(typeDiscovery.Id, typeDiscovery))
                {
                    model.AddDiagnostic(DiagnosticLevel.Warning, Name,
                        $"Duplicate type id skipped: {typeDiscovery.Id}");
                }
            }
        }
    }

    private static async IAsyncEnumerable<string> EnumerateSourceFilesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var file in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }

    private static TypeDiscovery? CreateTypeDiscovery(TypeDeclarationSyntax typeDecl, string filePath)
    {
        var name = typeDecl.Identifier.ValueText;
        if (string.IsNullOrEmpty(name)) return null;

        var namespaceDecl = typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        var namespaceName = namespaceDecl?.Name.ToString() ?? "global";
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
            Layer = InferLayer(namespaceName, filePath),
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
        foreach (var method in typeDecl.Members.OfType<MethodDeclarationSyntax>())
        {
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
            .Select(t => t.Type.ToString())
            .Where(t => !t.StartsWith("I"))
            .ToImmutableArray();
    }

    private static ImmutableArray<string> ExtractInterfaces(TypeDeclarationSyntax typeDecl)
    {
        if (typeDecl.BaseList == null) return [];

        return typeDecl.BaseList.Types
            .Select(t => t.Type.ToString())
            .Where(t => t.StartsWith("I"))
            .ToImmutableArray();
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

    private static ArchitectureLayer InferLayer(string namespaceName, string filePath)
    {
        var lowerNs = namespaceName.ToLowerInvariant();
        if (lowerNs.Contains("presentation") || lowerNs.Contains("ui") || lowerNs.Contains("web"))
            return ArchitectureLayer.Presentation;
        if (lowerNs.Contains("api") || lowerNs.Contains("controller") || lowerNs.Contains("endpoint"))
            return ArchitectureLayer.Api;
        if (lowerNs.Contains("application") || lowerNs.Contains("usecase") || lowerNs.Contains("mediatr"))
            return ArchitectureLayer.Application;
        if (lowerNs.Contains("domain") || lowerNs.Contains("model") || lowerNs.Contains("entity"))
            return ArchitectureLayer.Domain;
        if (lowerNs.Contains("infrastructure") || lowerNs.Contains("data") || lowerNs.Contains("persistence"))
            return ArchitectureLayer.Infrastructure;

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

        return ArchitectureLayer.Unknown;
    }
}
