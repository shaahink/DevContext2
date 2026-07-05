using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using DevContext.Core.Models;
using TypeKind = DevContext.Core.Models.TypeKind;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects MediatR handlers and marker interfaces (IRequest, ICommand, IQuery) via syntax tree analysis.</summary>
[ExtractorOrder(20)]
public sealed class MediatRExtractor : IDiscoveryExtractor
{
    private static readonly ImmutableArray<string> RequestMarkers =
        ["IRequest", "ICommand", "IQuery"];

    private static readonly ImmutableArray<string> HandlerBaseInterfaces =
        ["IRequestHandler", "INotificationHandler", "IStreamRequestHandler"];

    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "MediatRExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.MediatR], ["mediatr-handler-detections"],
        ["model.Detections"],
        "Walks syntax trees to detect MediatR handlers and marker interfaces");
    /// <summary>Runs when the MediatR package signal fired, OR when the code itself implements MediatR
    /// handler interfaces. The package signal misses repos that reference MediatR transitively or from
    /// another project in the closure (e.g. eShop's handlers in Ordering.API while the package lives in
    /// Ordering.Domain) — but the handler detections are exactly what the trace needs to bridge
    /// Send→handler, so detect them from the code regardless.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.MediatR)
            || ImplementsHandlerInterface(currentModel);

    private static bool ImplementsHandlerInterface(DiscoveryModel model)
    {
        var handlerSet = BuildHandlerInterfaceSet(model);
        foreach (var type in model.Types.Values)
        {
            foreach (var iface in type.ImplementedInterfaces)
            {
                var stripped = StripGenericsFrom(iface);
                if (handlerSet.Contains(stripped))
                    return true;
            }
        }
        return false;
    }

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        // M1.1: build handler-interface closure from the type model so derived interfaces
        // like ICommandHandler<,> (extends IRequestHandler<,>) are recognised.
        var handlerInterfaceSet = BuildHandlerInterfaceSet(model);

        foreach (var filePath in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();

            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDecl in classes)
            {
                var handlerType = classDecl.Identifier.ValueText;
                var baseTypes = classDecl.BaseList?.Types;

                if (baseTypes == null) continue;

                foreach (var baseType in baseTypes)
                {
                    var typeName = baseType.Type.ToString();

                    var match = TryParseHandlerType(typeName, handlerInterfaceSet);
                    if (match == null) continue;

                    var lineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    var (requestType, responseType, kind) = match.Value;

                    var detection = new MediatRHandlerDetection(
                        RequestType: requestType,
                        ResponseType: responseType,
                        HandlerType: handlerType,
                        Kind: kind)
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = lineNumber,
                    };

                    model.Detections.Add(detection);
                }
            }
        }
    }

    internal static (string RequestType, string ResponseType, MediatRKind Kind)? TryParseHandlerType(
        string typeName, HashSet<string> handlerInterfaceSet)
    {
        if (typeName.StartsWith("IRequestHandler<"))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length >= 2)
            {
                return (args[0], args[1], MediatRKind.Command);
            }
        }

        if (typeName.StartsWith("INotificationHandler<"))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length >= 1)
            {
                return (args[0], "Unit", MediatRKind.Notification);
            }
        }

        if (typeName is "IRequest" or "ICommand" or "IQuery")
        {
            return ("<self>", "Unit", MediatRKind.Command);
        }

        var baseName = ExtractGenericBaseName(typeName);
        if (baseName != null && RequestMarkers.Contains(baseName))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length == 1)
            {
                var kind = baseName switch
                {
                    "ICommand" => MediatRKind.Command,
                    "IQuery" => MediatRKind.Query,
                    _ => MediatRKind.Command,
                };
                return (args[0], "Unit", kind);
            }
            if (args.Length == 2)
            {
                var kind = baseName switch
                {
                    "ICommand" => MediatRKind.Command,
                    "IQuery" => MediatRKind.Query,
                    _ => MediatRKind.Command,
                };
                return (args[0], args[1], kind);
            }
        }

        // M1.1: interface-closure fallback — recognise derived handler interfaces
        // (e.g. ICommandHandler<X, bool> whose definition implements IRequestHandler<,>)
        if (baseName != null && handlerInterfaceSet.Contains(baseName))
        {
            var args = ExtractGenericArguments(typeName);
            if (args.Length >= 2)
            {
                var kind = baseName.Contains("Command", StringComparison.Ordinal) ? MediatRKind.Command
                    : baseName.Contains("Query", StringComparison.Ordinal) ? MediatRKind.Query
                    : MediatRKind.Command;
                return (args[0], args[1], kind);
            }
            if (args.Length == 1)
            {
                var kind = baseName.Contains("Notification", StringComparison.Ordinal) ? MediatRKind.Notification
                    : MediatRKind.Command;
                return (args[0], "Unit", kind);
            }
        }

        return null;
    }

    /// <summary>
    /// M1.1: builds the set of interface short names whose definition transitively derives from
    /// IRequestHandler / INotificationHandler / IStreamRequestHandler. This is the interface
    /// closure that allows recognising ICommandHandler&lt;,&gt; as a MediatR handler interface
    /// even though the syntax-level type name doesn't start with "IRequestHandler".
    /// </summary>
    internal static HashSet<string> BuildHandlerInterfaceSet(DiscoveryModel model)
    {
        var handlerSet = new HashSet<string>(StringComparer.Ordinal);
        foreach (var h in HandlerBaseInterfaces)
            handlerSet.Add(h);

        // Index types by short name (generics stripped) so parent interfaces can be resolved
        var byShortName = new Dictionary<string, List<TypeDiscovery>>(StringComparer.Ordinal);
        foreach (var t in model.Types.Values)
        {
            var sn = StripGenericsFrom(t.Name);
            if (!byShortName.TryGetValue(sn, out var list))
                byShortName[sn] = list = [];
            list.Add(t);
        }

        // Walk every interface definition and test whether it reaches a handler base
        foreach (var type in model.Types.Values)
        {
            if (type.Kind != TypeKind.Interface) continue;
            var sn = StripGenericsFrom(type.Name);
            if (handlerSet.Contains(sn)) continue;

            if (ReachesHandlerInterface(type, byShortName, handlerSet, []))
                handlerSet.Add(sn);
        }

        return handlerSet;
    }

    private static bool ReachesHandlerInterface(
        TypeDiscovery type,
        Dictionary<string, List<TypeDiscovery>> byShortName,
        HashSet<string> handlerSet,
        HashSet<string> visited)
    {
        foreach (var iface in type.ImplementedInterfaces)
        {
            var stripped = StripGenericsFrom(iface);
            if (handlerSet.Contains(stripped))
                return true;

            if (!visited.Add(stripped))
                continue;

            if (byShortName.TryGetValue(stripped, out var parents))
            {
                foreach (var parent in parents)
                {
                    if (parent.Kind == TypeKind.Interface
                        && ReachesHandlerInterface(parent, byShortName, handlerSet, visited))
                        return true;
                }
            }
        }

        // Also check BaseTypes (some patterns use abstract base classes that implement interfaces)
        foreach (var bt in type.BaseTypes)
        {
            var stripped = StripGenericsFrom(bt);
            if (!visited.Add(stripped))
                continue;

            if (byShortName.TryGetValue(stripped, out var bases))
            {
                foreach (var baseType in bases)
                {
                    if (ReachesHandlerInterface(baseType, byShortName, handlerSet, visited))
                        return true;
                }
            }
        }

        return false;
    }

    private static string StripGenericsFrom(string typeName)
    {
        var open = typeName.IndexOf('<');
        return open < 0 ? typeName : typeName[..open];
    }

    private static string[] ExtractGenericArguments(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        if (openBracket < 0) return [];

        var closeBracket = typeName.LastIndexOf('>');
        if (closeBracket <= openBracket) return [];

        var inner = typeName.Substring(openBracket + 1, closeBracket - openBracket - 1);
        return SplitGenericArgs(inner);
    }

    private static string? ExtractGenericBaseName(string typeName)
    {
        var openBracket = typeName.IndexOf('<');
        return openBracket < 0 ? typeName : typeName[..openBracket];
    }

    private static string[] SplitGenericArgs(string args)
    {
        var depth = 0;
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var ch in args)
        {
            switch (ch)
            {
                case '<':
                    depth++;
                    current.Append(ch);
                    break;
                case '>':
                    depth--;
                    current.Append(ch);
                    break;
                case ',' when depth == 0:
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                    break;
                default:
                    current.Append(ch);
                    break;
            }
        }

        if (current.Length > 0)
            parts.Add(current.ToString().Trim());

        return [.. parts];
    }
}
