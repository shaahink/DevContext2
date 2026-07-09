using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Graph2;

/// <summary>Single structured pass over method bodies that produces <see cref="BodyFacts"/> — the Loom
/// design's replacement for the 18 regex body-scan sites (§2.1). Pure and syntactic: it reads the
/// already-parsed tree (never re-parses), derives line numbers from the syntax tree (no char-offset
/// estimation), and never lets string literals enter the op stream. The enclosing member is known by
/// construction, so downstream seam detectors anchor edges correctly without guessing.</summary>
public static class BodyFactExtractor
{
    private static readonly HashSet<string> MappingMethods = new(StringComparer.Ordinal)
    {
        "Adapt", "Map", "MapTo", "MapFrom", "Create", "CreateFrom",
    };

    /// <summary>Extracts body facts for every method-like member declared under <paramref name="root"/>.</summary>
    public static ImmutableArray<BodyFacts> Extract(SyntaxNode root, string filePath, string project)
    {
        var result = ImmutableArray.CreateBuilder<BodyFacts>();

        foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            var typeFqn = GetTypeFullName(typeDecl);
            if (string.IsNullOrEmpty(typeFqn)) continue;

            var typeScope = BuildTypeScope(typeDecl);
            var methodReturns = BuildMethodReturns(typeDecl);

            foreach (var member in typeDecl.Members)
            {
                var (name, paramCount, body) = DescribeMember(member);
                if (body is null) continue;

                var ops = WalkMember(member, body, typeScope, methodReturns, filePath, project);
                var memberId = new SymbolId(SymbolKind.Member, $"{typeFqn}::{name}({paramCount})");
                result.Add(new BodyFacts(memberId, name, ops) { File = filePath, Project = project });
            }
        }

        return result.ToImmutable();
    }

    /// <summary>Extracts body facts from a parsed syntax tree.</summary>
    public static ImmutableArray<BodyFacts> Extract(SyntaxTree tree, string filePath, string project)
        => Extract(tree.GetRoot(), filePath, project);

    private static (string Name, int ParamCount, SyntaxNode? Body) DescribeMember(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax m => (m.Identifier.ValueText, m.ParameterList.Parameters.Count,
            (SyntaxNode?)m.Body ?? m.ExpressionBody),
        ConstructorDeclarationSyntax c => (c.Identifier.ValueText, c.ParameterList.Parameters.Count,
            (SyntaxNode?)c.Body ?? c.ExpressionBody),
        _ => ("", 0, null),
    };

    private static ImmutableArray<BodyOp> WalkMember(
        MemberDeclarationSyntax member, SyntaxNode body,
        Dictionary<string, string> typeScope, Dictionary<string, string> methodReturns,
        string filePath, string project)
    {
        // A member-local scope of identifier → declared/inferred type text, seeded from the type scope
        // (fields, properties, primary-ctor params) then extended with this member's params, lambda
        // params, and local declarations. Receiver/argument types resolve against this — no field regex.
        var scope = new Dictionary<string, string>(typeScope, StringComparer.Ordinal);

        if (member is MethodDeclarationSyntax md)
            foreach (var p in md.ParameterList.Parameters) AddParam(scope, p);
        if (member is ConstructorDeclarationSyntax cd)
            foreach (var p in cd.ParameterList.Parameters) AddParam(scope, p);

        // Pass 0: locals populate the scope so later uses (and receivers) resolve regardless of order.
        foreach (var decl in body.DescendantNodes().OfType<VariableDeclarationSyntax>())
        {
            var declaredType = decl.Type.IsVar ? null : decl.Type.ToString();
            foreach (var v in decl.Variables)
            {
                var inferred = InferInitializer(v.Initializer?.Value, methodReturns);
                var effective = inferred ?? (declaredType is null ? null : StripGenerics(declaredType));
                if (!string.IsNullOrEmpty(effective)) scope[v.Identifier.ValueText] = effective!;
            }
        }

        // Pass 2: emit ops in document order with resolved receiver/argument types.
        var ops = ImmutableArray.CreateBuilder<BodyOp>();
        var seenIdentifiers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in body.DescendantNodes())
        {
            switch (node)
            {
                case VariableDeclarationSyntax decl:
                {
                    var declaredTypeText = decl.Type.IsVar ? null : decl.Type.ToString();
                    foreach (var v in decl.Variables)
                    {
                        var inferred = InferInitializer(v.Initializer?.Value, methodReturns);
                        ops.Add(new LocalDeclOp(
                            Line(v),
                            v.Identifier.ValueText,
                            declaredTypeText is null ? null : Ref(StripGenerics(declaredTypeText), v, filePath, project),
                            inferred is null ? null : Ref(inferred, v, filePath, project)));
                    }
                    break;
                }
                case ObjectCreationExpressionSyntax oc:
                    ops.Add(new CreationOp(Line(oc), Ref(StripGenerics(oc.Type.ToString()), oc, filePath, project)));
                    break;
                case InvocationExpressionSyntax inv:
                    ops.Add(BuildInvocation(inv, scope, methodReturns, filePath, project));
                    break;
                case IdentifierNameSyntax id:
                {
                    var text = id.Identifier.ValueText;
                    if (seenIdentifiers.Add(text))
                        ops.Add(new IdentifierUseOp(Line(id), text));
                    break;
                }
            }
        }

        return ops.ToImmutable();
    }

    private static InvocationOp BuildInvocation(
        InvocationExpressionSyntax inv, Dictionary<string, string> scope,
        Dictionary<string, string> methodReturns, string filePath, string project)
    {
        string? receiverText = null;
        string methodName;
        var genericArgs = ImmutableArray<SymbolRef>.Empty;

        switch (inv.Expression)
        {
            case MemberAccessExpressionSyntax ma:
                (methodName, genericArgs) = SplitName(ma.Name, inv, filePath, project);
                receiverText = RootIdentifier(ma.Expression);
                break;
            case GenericNameSyntax gn:
                (methodName, genericArgs) = SplitName(gn, inv, filePath, project);
                break;
            case IdentifierNameSyntax idn:
                methodName = idn.Identifier.ValueText;
                break;
            default:
                methodName = inv.Expression.ToString();
                break;
        }

        SymbolRef? receiverType = null;
        if (receiverText is not null)
        {
            var rt = ResolveFromScope(scope, inv, receiverText);
            if (rt is not null)
                receiverType = Ref(StripGenerics(rt), inv, filePath, project);
        }

        var args = ImmutableArray.CreateBuilder<ArgFact>();
        foreach (var arg in inv.ArgumentList.Arguments)
        {
            var argText = RootIdentifier(arg.Expression) ?? arg.Expression.ToString();
            SymbolRef? argType = null;
            var at = ResolveFromScope(scope, inv, argText);
            if (at is not null)
                argType = Ref(StripGenerics(at), arg, filePath, project);
            else if (InferInitializer(arg.Expression, methodReturns) is { } inlineType)
                argType = Ref(inlineType, arg, filePath, project); // inline `new X(...)` / `.Adapt<X>()`
            args.Add(new ArgFact(argText, argType));
        }

        return new InvocationOp(Line(inv), receiverText, receiverType, methodName, genericArgs, args.ToImmutable());
    }

    private static (string Name, ImmutableArray<SymbolRef> Generics) SplitName(
        SimpleNameSyntax name, SyntaxNode site, string filePath, string project)
    {
        if (name is GenericNameSyntax gn)
        {
            var generics = gn.TypeArgumentList.Arguments
                .Select(t => Ref(StripGenerics(t.ToString()), site, filePath, project))
                .ToImmutableArray();
            return (gn.Identifier.ValueText, generics);
        }
        return (name.Identifier.ValueText, ImmutableArray<SymbolRef>.Empty);
    }

    /// <summary>The statically-obvious yielded type of an initializer, for exactly the cases the old
    /// regexes chased: <c>new X()</c>, <c>expr.Adapt&lt;X&gt;()</c>/<c>Map&lt;X&gt;()</c>/<c>Create&lt;X&gt;()</c>,
    /// awaited variants, and calls to a same-type method whose return type is written explicitly
    /// (e.g. <c>var command = MapToCreateOrderCommand(evt)</c>). Anything else returns null (TierB
    /// semantic resolves it later, or it stays unresolved) — never a right-to-left text scan.</summary>
    private static string? InferInitializer(ExpressionSyntax? expr, Dictionary<string, string> methodReturns)
    {
        while (expr is AwaitExpressionSyntax ae) expr = ae.Expression;

        switch (expr)
        {
            case ObjectCreationExpressionSyntax oc:
                return StripGenerics(oc.Type.ToString());
            case InvocationExpressionSyntax inv:
            {
                var name = inv.Expression switch
                {
                    MemberAccessExpressionSyntax ma => ma.Name,
                    SimpleNameSyntax sn => sn,
                    _ => null,
                };
                if (name is GenericNameSyntax gn
                    && gn.TypeArgumentList.Arguments.Count == 1
                    && MappingMethods.Contains(gn.Identifier.ValueText))
                {
                    return StripGenerics(gn.TypeArgumentList.Arguments[0].ToString());
                }

                // Call to a method declared in the same type — its written return type is a Tier-A fact.
                var bareName = inv.Expression switch
                {
                    IdentifierNameSyntax idn => idn.Identifier.ValueText,
                    MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } tma => tma.Name.Identifier.ValueText,
                    _ => null,
                };
                if (bareName is not null && methodReturns.TryGetValue(bareName, out var ret))
                    return ret; // already unwrapped + stripped by BuildMethodReturns

                return null;
            }
            default:
                return null;
        }
    }

    /// <summary>Map of method short-name → its declared return type (Task/ValueTask unwrapped, generics
    /// stripped), skipping void/Task-returning methods. Enables same-file return-type inference.</summary>
    private static Dictionary<string, string> BuildMethodReturns(TypeDeclarationSyntax typeDecl)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var m in typeDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            var ret = UnwrapReturnType(m.ReturnType.ToString());
            if (string.IsNullOrEmpty(ret) || ret is "void" or "Task" or "ValueTask") continue;
            map[m.Identifier.ValueText] = ret;
        }
        return map;
    }

    private static string UnwrapReturnType(string returnType)
    {
        var t = returnType.Trim();
        foreach (var wrapper in (ReadOnlySpan<string>)["Task<", "ValueTask<", "ActionResult<"])
        {
            if (t.StartsWith(wrapper, StringComparison.Ordinal))
            {
                var inner = t[wrapper.Length..].TrimEnd('>');
                return StripGenerics(inner);
            }
        }
        return StripGenerics(t);
    }

    private static Dictionary<string, string> BuildTypeScope(TypeDeclarationSyntax typeDecl)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var field in typeDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            var t = StripGenerics(field.Declaration.Type.ToString());
            foreach (var v in field.Declaration.Variables) map[v.Identifier.ValueText] = t;
        }
        foreach (var prop in typeDecl.Members.OfType<PropertyDeclarationSyntax>())
            map[prop.Identifier.ValueText] = StripGenerics(prop.Type.ToString());

        // Primary constructor params (C# 12 class/record) behave like fields for resolution.
        if (typeDecl.ParameterList is not null)
            foreach (var p in typeDecl.ParameterList.Parameters) AddParam(map, p);

        return map;
    }

    private static void AddParam(Dictionary<string, string> scope, ParameterSyntax? p)
    {
        if (p?.Type is null) return;
        scope[p.Identifier.ValueText] = StripGenerics(p.Type.ToString());
    }

    private static string? ResolveFromScope(Dictionary<string, string> scope, SyntaxNode node, string name)
    {
        if (scope.TryGetValue(name, out var type)) return type;
        return GetEnclosingParamType(node, name);
    }

    private static string? GetEnclosingParamType(SyntaxNode node, string name)
    {
        for (var parent = node.Parent; parent != null; parent = parent.Parent)
        {
            switch (parent)
            {
                case ParenthesizedLambdaExpressionSyntax pl:
                {
                    foreach (var prm in pl.ParameterList.Parameters)
                        if (prm.Type is not null && prm.Identifier.ValueText == name)
                            return StripGenerics(prm.Type.ToString());
                    break;
                }
                case SimpleLambdaExpressionSyntax sl:
                {
                    if (sl.Parameter is { Type: not null } prm && prm.Identifier.ValueText == name)
                        return StripGenerics(prm.Type.ToString());
                    break;
                }
                case LocalFunctionStatementSyntax lf:
                {
                    foreach (var prm in lf.ParameterList.Parameters)
                        if (prm.Type is not null && prm.Identifier.ValueText == name)
                            return StripGenerics(prm.Type.ToString());
                    break;
                }
            }
        }
        return null;
    }

    private static string? RootIdentifier(ExpressionSyntax expr)
    {
        while (true)
        {
            switch (expr)
            {
                case MemberAccessExpressionSyntax ma:
                    expr = ma.Expression;
                    continue;
                case InvocationExpressionSyntax inv:
                    expr = inv.Expression;
                    continue;
                case IdentifierNameSyntax id:
                    return id.Identifier.ValueText;
                case ThisExpressionSyntax:
                    return "this";
                default:
                    return null;
            }
        }
    }

    private static SymbolRef Ref(string text, SyntaxNode site, string filePath, string project) => new()
    {
        Text = text,
        Site = new RefSite { File = filePath, Line = Line(site), Project = project },
    };

    private static int Line(SyntaxNode node) => node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static string StripGenerics(string typeName)
    {
        var idx = typeName.IndexOf('<');
        var t = idx > 0 ? typeName[..idx] : typeName;
        return t.Trim();
    }

    private static string GetTypeFullName(TypeDeclarationSyntax typeDecl)
    {
        var ns = typeDecl.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString();
        return ns is null ? typeDecl.Identifier.ValueText : $"{ns}.{typeDecl.Identifier.ValueText}";
    }
}
