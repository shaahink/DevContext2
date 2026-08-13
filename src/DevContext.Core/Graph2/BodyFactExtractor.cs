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
                result.Add(new BodyFacts(memberId, name, ops)
                {
                    File = filePath,
                    Project = project,
                    DeclLine = Line(member),
                });
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
        // Scope values carry the RAW type text (generic args intact) — Ref() derives base + arity.
        foreach (var decl in body.DescendantNodes().OfType<VariableDeclarationSyntax>())
        {
            var declaredType = decl.Type.IsVar ? null : decl.Type.ToString();
            foreach (var v in decl.Variables)
            {
                var inferred = InferInitializer(v.Initializer?.Value, methodReturns);
                var effective = inferred ?? declaredType;
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
                            declaredTypeText is null ? null : Ref(declaredTypeText, v, filePath, project),
                            inferred is null ? null : Ref(inferred, v, filePath, project))
                        { Span = v.Span });
                    }
                    break;
                }
                case ObjectCreationExpressionSyntax oc:
                    ops.Add(new CreationOp(Line(oc), Ref(oc.Type.ToString(), oc, filePath, project)) { Span = oc.Span });
                    break;
                case InvocationExpressionSyntax inv:
                    ops.Add(BuildInvocation(inv, scope, methodReturns, filePath, project));
                    break;
                case IdentifierNameSyntax id:
                {
                    var text = id.Identifier.ValueText;
                    if (seenIdentifiers.Add(text))
                        ops.Add(new IdentifierUseOp(Line(id), text) { Span = id.Span });
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
        string? receiverMember = null;
        string methodName;
        var genericArgs = ImmutableArray<SymbolRef>.Empty;

        switch (inv.Expression)
        {
            case MemberAccessExpressionSyntax ma:
                (methodName, genericArgs) = SplitName(ma.Name, inv, filePath, project);
                (receiverText, receiverMember) = SplitReceiver(ma.Expression);
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
                receiverType = Ref(rt, inv, filePath, project);
        }

        var args = ImmutableArray.CreateBuilder<ArgFact>();
        foreach (var arg in inv.ArgumentList.Arguments)
        {
            var argText = RootIdentifier(arg.Expression) ?? arg.Expression.ToString();
            SymbolRef? argType = null;
            var at = ResolveFromScope(scope, inv, argText);
            if (at is not null)
                argType = Ref(at, arg, filePath, project);
            else if (InferInitializer(arg.Expression, methodReturns) is { } inlineType)
                argType = Ref(inlineType, arg, filePath, project); // inline `new X(...)` / `.Adapt<X>()`
            args.Add(new ArgFact(argText, argType));
        }

        return new InvocationOp(Line(inv), receiverText, receiverType, methodName, genericArgs, args.ToImmutable())
        {
            ReceiverMember = receiverMember,
            // E1.2 (#12): the invocation's OWN span, so the semantic upgrade relocates THIS call and not
            // whatever statement happens to share its line.
            Span = inv.Span,
        };
    }

    private static (string Name, ImmutableArray<SymbolRef> Generics) SplitName(
        SimpleNameSyntax name, SyntaxNode site, string filePath, string project)
    {
        if (name is GenericNameSyntax gn)
        {
            var generics = gn.TypeArgumentList.Arguments
                .Select(t => Ref(t.ToString(), site, filePath, project))
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
                return oc.Type.ToString();
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
                    return gn.TypeArgumentList.Arguments[0].ToString();
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

    /// <summary>Map of method short-name → its declared return type text (Task/ValueTask unwrapped,
    /// raw — generic args intact so use-site arity survives to the ref), skipping void/Task-returning
    /// methods. Enables same-file return-type inference.</summary>
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
            if (t.StartsWith(wrapper, StringComparison.Ordinal) && t.EndsWith('>'))
                return t[wrapper.Length..^1].Trim();
        }
        return t;
    }

    private static Dictionary<string, string> BuildTypeScope(TypeDeclarationSyntax typeDecl)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var field in typeDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            var t = field.Declaration.Type.ToString();
            foreach (var v in field.Declaration.Variables) map[v.Identifier.ValueText] = t;
        }
        foreach (var prop in typeDecl.Members.OfType<PropertyDeclarationSyntax>())
            map[prop.Identifier.ValueText] = prop.Type.ToString();

        // Primary constructor params (C# 12 class/record) behave like fields for resolution.
        if (typeDecl.ParameterList is not null)
            foreach (var p in typeDecl.ParameterList.Parameters) AddParam(map, p);

        return map;
    }

    private static void AddParam(Dictionary<string, string> scope, ParameterSyntax? p)
    {
        if (p?.Type is null) return;
        scope[p.Identifier.ValueText] = p.Type.ToString();
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
                            return prm.Type.ToString();
                    break;
                }
                case SimpleLambdaExpressionSyntax sl:
                {
                    if (sl.Parameter is { Type: not null } prm && prm.Identifier.ValueText == name)
                        return prm.Type.ToString();
                    break;
                }
                case LocalFunctionStatementSyntax lf:
                {
                    foreach (var prm in lf.ParameterList.Parameters)
                        if (prm.Type is not null && prm.Identifier.ValueText == name)
                            return prm.Type.ToString();
                    break;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Splits a receiver expression into the identifier its TYPE resolves from and the trailing segment
    /// before the call. Two spelling normalisations, both so that one call produces one fact:
    /// <list type="bullet">
    /// <item>A receiver chain keeps its trailing segment — <c>services.Mediator.Send(c)</c> resolves
    /// through root "services" and reports "Mediator", so dispatch detection and the binder's
    /// receiver-chain hop see the member, not just the root.</item>
    /// <item>An explicit <c>this.</c> is dropped. D-3: <c>this.service.Call()</c> and
    /// <c>service.Call()</c> are the same call, but <see cref="RootIdentifier"/> bottoms out at the
    /// <c>this</c> TOKEN, and "this" matches no entry in the type scope — so the qualified spelling lost
    /// its receiver type entirely while the bare one resolved. That cost GitVersion's CLI every one of
    /// its handler joins (16 of 17 invocation sites across five verbs produced no receiver type at all).
    /// The member directly on <c>this</c> IS a scope entry: <see cref="BuildTypeScope"/> already holds
    /// every field, property and primary-constructor parameter with its declared type.</item>
    /// </list>
    /// Two structural conditions, no name list: the walk must reach <c>this</c> through member accesses
    /// ONLY (so <c>this.Make().Do()</c> — whose segment is a call result, not a declared member — is left
    /// alone), and there must be a segment to unwrap (so a bare <c>this.Helper()</c> self-call still
    /// reports "this" and still reaches the call binder's declares-the-method self-call arm).
    /// </summary>
    private static (string? Text, string? Member) SplitReceiver(ExpressionSyntax receiver)
    {
        var member = receiver is MemberAccessExpressionSyntax ma ? ma.Name.Identifier.ValueText : null;
        var text = RootIdentifier(receiver);
        if (text is not "this") return (text, member);

        var expr = receiver;
        string? memberOnThis = null;
        var segments = 0;
        while (expr is MemberAccessExpressionSyntax hop)
        {
            memberOnThis = hop.Name.Identifier.ValueText;
            expr = hop.Expression;
            segments++;
        }
        if (expr is not ThisExpressionSyntax || segments == 0) return (text, member);

        // `this.x.M()` → the receiver IS x, and naming it twice would send the chain hop looking for a
        // property "x" on x's own type. `this.x.y.M()` → root x, trailing y, exactly like `x.y.M()`.
        return segments == 1 ? (memberOnThis, null) : (memberOnThis, member);
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

    /// <summary>Builds a ref from RAW type text: Text is the bare base name (detector catalogs match
    /// on it), the use-site generic arity rides separately so <see cref="SymbolTable"/> can pick the
    /// structurally-matching declaration (<c>new IdentifiedCommand&lt;T, R&gt;(…)</c> → arity-2 type).</summary>
    private static SymbolRef Ref(string text, SyntaxNode site, string filePath, string project)
    {
        var (baseName, arity) = SymbolCanon.SplitGenericText(text);
        return new()
        {
            Text = baseName,
            Site = new RefSite { File = filePath, Line = Line(site), Project = project },
            Arity = arity,
        };
    }

    // Mapped span honors razor virtual trees' #line directives (identical to the unmapped span for
    // ordinary .cs files) — op lines and edge provenance land on the TRUE razor lines.
    private static int Line(SyntaxNode node) => node.GetLocation().GetMappedLineSpan().StartLinePosition.Line + 1;

    private static string GetTypeFullName(TypeDeclarationSyntax typeDecl)
        // One canonical algebra with TypeDiscovery.Id (SymbolCanon: "global" prefix, nested-type
        // chain, generic arity) — a BodyFacts member id must never diverge from the graph node id
        // of its declaring type, or seam edges orphan from the entry's handler node (T2.5).
        => SymbolCanon.ForTypeDecl(typeDecl);
}
