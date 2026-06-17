using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects common anti-patterns: fire-and-forget tasks, IServiceScopeFactory, new outside DI, CancellationToken.None, unbounded collections.</summary>
[ExtractorOrder(45)]
public sealed class AntiPatternDetector : IDiscoveryExtractor
{
    public string Name => "AntiPatternDetector";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["model.Detections"],
        "Detects fire-and-forget tasks, IServiceScopeFactory, new outside DI, CancellationToken.None, unbounded collections");

    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => context.Options.IncludeAntiPatterns;

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct).ConfigureAwait(false))
        {
            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}: {ex.Message}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            // Single walk collects all needed node types
            var collector = new AntiPatternNodeCollector();
            collector.Visit(root);

            DetectFireAndForget(collector, filePath, model);
            DetectServiceScopeFactory(collector.Invocations, filePath, model);
            DetectNewOutsideDI(collector.ObjectCreations, filePath, model);
            DetectCancellationTokenNone(collector, filePath, model);
            DetectUnboundedCollections(collector.Fields, filePath, model);
            DetectAsyncVoid(collector.Methods, filePath, model);
        }

        DetectCaptiveDependency(model);
    }

    /// <summary>Collects syntax nodes in a single Roslyn walk to avoid repeated <c>DescendantNodes()</c> calls.</summary>
    private sealed class AntiPatternNodeCollector : CSharpSyntaxWalker
    {
        public readonly List<AssignmentExpressionSyntax> Assignments = [];
        public readonly List<InvocationExpressionSyntax> Invocations = [];
        public readonly List<ObjectCreationExpressionSyntax> ObjectCreations = [];
        public readonly List<MemberAccessExpressionSyntax> MemberAccesses = [];
        public readonly List<DefaultExpressionSyntax> DefaultExpressions = [];
        public readonly List<FieldDeclarationSyntax> Fields = [];
        public readonly List<MethodDeclarationSyntax> Methods = [];

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            Assignments.Add(node);
            base.VisitAssignmentExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            Invocations.Add(node);
            base.VisitInvocationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            ObjectCreations.Add(node);
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            MemberAccesses.Add(node);
            base.VisitMemberAccessExpression(node);
        }

        public override void VisitDefaultExpression(DefaultExpressionSyntax node)
        {
            DefaultExpressions.Add(node);
            base.VisitDefaultExpression(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            Fields.Add(node);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            Methods.Add(node);
            base.VisitMethodDeclaration(node);
        }
    }

    private static void DetectFireAndForget(AntiPatternNodeCollector collector, string filePath, DiscoveryModel model)
    {
        var isTestFile = ExtractorHelpers.IsTestFile(filePath);
        var severity = isTestFile ? "low" : "high";
        var suffix = isTestFile ? " [test file — likely intentional]" : "";

        foreach (var assignment in collector.Assignments)
        {
            if (string.Equals(assignment.Left.ToString(), "_", StringComparison.Ordinal) &&
                assignment.Right is InvocationExpressionSyntax inv)
            {
                var methodName = GetMethodName(inv);
                var line = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "FireAndForget",
                    $"Discard assignment to `{methodName}` — task is never awaited. Exceptions may be lost.{suffix}",
                    severity, methodName)
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }

        foreach (var inv in collector.Invocations)
        {
            if (inv.Parent is not ExpressionStatementSyntax exprStmt) continue;
            if (inv == exprStmt.Expression && inv.Parent?.Parent is not (BlockSyntax or ArrowExpressionClauseSyntax or CompilationUnitSyntax))
                continue;

            var exprStr = inv.Expression.ToString();
            var isTaskRun = exprStr.Contains("Task.Run", StringComparison.Ordinal) || exprStr.Contains("Task.Factory.StartNew", StringComparison.Ordinal);
            var isContinueWith = exprStr.Contains(".ContinueWith", StringComparison.Ordinal);

            if (!isTaskRun && !isContinueWith) continue;

            if (isTaskRun)
            {
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "FireAndForget",
                    $"`{Truncate(exprStr, 60)}` without await — task runs unobserved. Exceptions may be lost in thread pool.",
                    "high", "Task.Run")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
            else
            {
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "FireAndForget",
                    "ContinueWith without await — unobserved task continuation.",
                    "high", "ContinueWith")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }
    }

    private static void DetectServiceScopeFactory(List<InvocationExpressionSyntax> invocations, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var inv in invocations)
        {
            if (inv.Expression is not MemberAccessExpressionSyntax memberAccess) continue;
            var methodName = memberAccess.Name.Identifier.ValueText;
            if (methodName is not ("CreateScope" or "CreateAsyncScope")) continue;

            var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            model.Detections.Add(new AntiPatternDetection(
                "ServiceLocator",
                $"IServiceScopeFactory.{methodName}() — manual service location. Prefer constructor injection.",
                "high", "IServiceScopeFactory")
            {
                ExtractorName = "AntiPatternDetector",
                SourceFile = filePath,
                LineNumber = line
            });
        }
    }

    private static void DetectNewOutsideDI(List<ObjectCreationExpressionSyntax> objectCreations, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var objCreation in objectCreations)
        {
            if (IsInConstructorOrDI(objCreation)) continue;

            var typeName = objCreation.Type.ToString();
            if (typeName is "ArgumentNullException" or "InvalidOperationException" or "NotSupportedException"
                or "List" or "Dictionary" or "ConcurrentDictionary" or "StringBuilder"
                or "CancellationTokenSource" or "JsonSerializerOptions" or "DbContextOptionsBuilder"
                or "SqliteConnection" or "DbSet" or "ConfigurationBuilder" or "LoggerConfiguration"
                or "Exception" or "AggregateException" or "SqlException")
                continue;

            if (typeName.EndsWith("Mock", StringComparison.Ordinal)
                || typeName.EndsWith("MockService", StringComparison.Ordinal)
                || typeName.EndsWith("Test", StringComparison.Ordinal))
                continue;

            if (!IsLikelyService(typeName)) continue;

            var line = objCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            model.Detections.Add(new AntiPatternDetection(
                "NewOutsideDI",
                $"`new {typeName}(...)` outside constructor/DI — not managed by container. Hard to test/mock.",
                "medium", typeName)
            {
                ExtractorName = "AntiPatternDetector",
                SourceFile = filePath,
                LineNumber = line
            });
        }
    }

    private static void DetectCancellationTokenNone(AntiPatternNodeCollector collector, string filePath, DiscoveryModel model)
    {
        var severity = ExtractorHelpers.IsTestFile(filePath) ? "low" : "medium";

        foreach (var memberAccess in collector.MemberAccesses)
        {
            if (memberAccess.ToString().EndsWith("CancellationToken.None", StringComparison.Ordinal))
            {
                var line = memberAccess.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "CancellationTokenNone",
                    "`CancellationToken.None` used — operation cannot be cancelled.",
                    severity, "CancellationToken.None")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }

        foreach (var defaultExpr in collector.DefaultExpressions)
        {
            if (defaultExpr.Type.ToString() is "CancellationToken" or "System.Threading.CancellationToken")
            {
                var line = defaultExpr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "CancellationTokenNone",
                    "`default(CancellationToken)` — semantically equivalent to None. Operation cannot be cancelled.",
                    severity, "default(CancellationToken)")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }

        foreach (var objCreation in collector.ObjectCreations)
        {
            if (objCreation.Type.ToString() is "CancellationToken" or "System.Threading.CancellationToken"
                && objCreation.ArgumentList?.Arguments.Count is 0 or null)
            {
                var line = objCreation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "CancellationTokenNone",
                    "`new CancellationToken()` — default token, same as None. Operation cannot be cancelled.",
                    severity, "new CancellationToken()")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }
    }

    private static void DetectUnboundedCollections(List<FieldDeclarationSyntax> fields, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var field in fields)
        {
            var fieldType = field.Declaration.Type.ToString();
            if ((fieldType.Contains("ConcurrentDictionary", StringComparison.Ordinal) ||
                 fieldType.Contains("ConcurrentBag", StringComparison.Ordinal) ||
                 fieldType.Contains("ConcurrentQueue", StringComparison.Ordinal))
                && !fieldType.Contains("Channel", StringComparison.Ordinal))
            {
                var className = field.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText;
                var fullClass = className ?? "unknown";
                var hasCleanup = field.Ancestors().OfType<TypeDeclarationSyntax>()
                    .SelectMany(t => t.Members.OfType<MethodDeclarationSyntax>())
                    .Any(m => m.Identifier.ValueText.Contains("Clear", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Cleanup", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Prune", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Evict", StringComparison.OrdinalIgnoreCase));

                var line = field.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var noteHasCleanup = hasCleanup ? " (cleanup method found)" : " — no eviction/cleanup method found";

                foreach (var variable in field.Declaration.Variables)
                {
                    model.Detections.Add(new AntiPatternDetection(
                        "UnboundedCollection",
                        $"`{fieldType} {variable.Identifier.ValueText}` in `{fullClass}`{noteHasCleanup}.",
                        hasCleanup ? "low" : "medium", variable.Identifier.ValueText)
                    {
                        ExtractorName = "AntiPatternDetector",
                        SourceFile = filePath,
                        LineNumber = line
                    });
                }
            }
        }
    }

    private static bool IsInConstructorOrDI(ObjectCreationExpressionSyntax objCreation)
    {
        var ancestor = objCreation.Ancestors()
            .FirstOrDefault(a => a is ConstructorDeclarationSyntax
                or FieldDeclarationSyntax
                or PropertyDeclarationSyntax
                or LambdaExpressionSyntax
                or AnonymousFunctionExpressionSyntax
                or ArrowExpressionClauseSyntax);

        // In constructor or field init: OK
        if (ancestor is ConstructorDeclarationSyntax or FieldDeclarationSyntax or PropertyDeclarationSyntax or ArrowExpressionClauseSyntax)
            return true;

        // In lambda (DI registration): OK
        if (ancestor is LambdaExpressionSyntax or AnonymousFunctionExpressionSyntax)
            return true;

        return false;
    }

    private static bool IsLikelyService(string typeName)
    {
        var lower = typeName.ToLowerInvariant();
        return lower.Contains("service", StringComparison.Ordinal) || lower.Contains("handler", StringComparison.Ordinal) || lower.Contains("manager", StringComparison.Ordinal)
            || lower.Contains("orchestrator", StringComparison.Ordinal) || lower.Contains("worker", StringComparison.Ordinal) || lower.Contains("runner", StringComparison.Ordinal)
            || lower.Contains("provider", StringComparison.Ordinal) || lower.Contains("resolver", StringComparison.Ordinal) || lower.Contains("factory", StringComparison.Ordinal)
            || lower.Contains("repository", StringComparison.Ordinal) || lower.Contains("dispatcher", StringComparison.Ordinal) || lower.Contains("tracker", StringComparison.Ordinal)
            || lower.Contains("adapter", StringComparison.Ordinal) || lower.Contains("broker", StringComparison.Ordinal);
    }

    private static string GetMethodName(InvocationExpressionSyntax inv)
    {
        return inv.Expression switch
        {
            MemberAccessExpressionSyntax m => m.Name.Identifier.ValueText,
            IdentifierNameSyntax id => id.Identifier.ValueText,
            _ => inv.Expression.ToString()
        };
    }

    private static string Truncate(string text, int maxLen) =>
        text.Length <= maxLen ? text : text[..(maxLen - 3)] + "...";

    private static void DetectAsyncVoid(List<MethodDeclarationSyntax> methods, string filePath, DiscoveryModel model)
    {
        foreach (var method in methods)
        {
            if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword))
                continue;

            if (method.ReturnType is not PredefinedTypeSyntax rt
                || !rt.Keyword.IsKind(SyntaxKind.VoidKeyword))
                continue;

            // Skip event handler signatures: (object sender, XxxEventArgs e)
            if (method.ParameterList.Parameters.Count >= 2
                && method.ParameterList.Parameters[1].Type?.ToString().Contains("EventArgs", StringComparison.Ordinal) == true)
                continue;

            var line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            model.Detections.Add(new AntiPatternDetection(
                "AsyncVoid",
                $"`{method.Identifier.ValueText}` is async void — exceptions are unobservable and crash the process.",
                "high", method.Identifier.ValueText)
            {
                ExtractorName = "AntiPatternDetector",
                SourceFile = filePath,
                LineNumber = line
            });
        }
    }

    private static void DetectCaptiveDependency(DiscoveryModel model)
    {
        // Build lifetime map from DI registrations
        var lifetimeMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var di in model.Detections.OfType<DiRegistrationDetection>())
        {
            if (di.Lifetime is "Singleton" or "Scoped" or "Transient"
                && !string.IsNullOrEmpty(di.ServiceType) && !string.Equals(di.ServiceType, "?"
, StringComparison.Ordinal) && !di.ServiceType.StartsWith("Add", StringComparison.Ordinal))
            {
                var svc = di.ServiceType.Trim();
                lifetimeMap[svc] = di.Lifetime;
            }
        }

        // Check types for CaptiveDependency: Singleton → Scoped dependency
        foreach (var type in model.Types.Values)
        {
            var ctors = type.Methods.Where(m => string.Equals(m.Name, ".ctor", StringComparison.Ordinal) || string.Equals(m.Name, type.Name, StringComparison.Ordinal)).ToList();
            if (ctors.Count == 0) continue;

            foreach (var paramType in ctors[0].ParameterTypes)
            {
                var trimmed = paramType.Trim();
                if (lifetimeMap.TryGetValue(trimmed, out var depLifetime)
                    && string.Equals(depLifetime, "Scoped"
, StringComparison.Ordinal) && lifetimeMap.TryGetValue(type.Name, out var selfLifetime)
                    && string.Equals(selfLifetime, "Singleton", StringComparison.Ordinal))
                {
                    model.Detections.Add(new AntiPatternDetection(
                        "CaptiveDependency",
                        $"`{type.Name}` (Singleton) depends on `{trimmed}` (Scoped). This causes scoped services to live as long as the singleton — a Captive Dependency.",
                        "high", type.Name)
                    {
                        ExtractorName = "AntiPatternDetector",
                        SourceFile = type.FilePath ?? "",
                        LineNumber = 0
                    });
                    break;
                }
            }
        }
    }
}
