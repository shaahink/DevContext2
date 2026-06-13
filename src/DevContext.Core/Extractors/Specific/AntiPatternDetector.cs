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
        await foreach (var filePath in ExtractorHelpers.EnumerateSourceFilesAsync(context, ct))
        {
            SyntaxTree syntaxTree;
            try
            {
                syntaxTree = await context.Cache.GetSyntaxTreeAsync(filePath, ct);
            }
            catch (Exception ex)
            {
                model.AddDiagnostic(DiagnosticLevel.Warning, Name, $"Failed to parse {filePath}: {ex.Message}");
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            DetectFireAndForget(root, filePath, model);
            DetectServiceScopeFactory(root, filePath, model);
            DetectNewOutsideDI(root, filePath, model);
            DetectCancellationTokenNone(root, filePath, model);
            DetectUnboundedCollections(root, filePath, model);
            DetectAsyncVoid(root, filePath, model);
        }

        DetectCaptiveDependency(model);
    }

    private static void DetectFireAndForget(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        var isTestFile = ExtractorHelpers.IsTestFile(filePath);
        var severity = isTestFile ? "low" : "high";
        var suffix = isTestFile ? " [test file — likely intentional]" : "";

        foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left.ToString() == "_" &&
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

        // Pattern: bare expression-statement calling Task.Run, Task.Factory.StartNew, or ContinueWith
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Parent is not ExpressionStatementSyntax exprStmt) continue;
            if (inv == exprStmt.Expression && inv.Parent?.Parent is not (BlockSyntax or ArrowExpressionClauseSyntax or CompilationUnitSyntax))
                continue;

            var exprStr = inv.Expression.ToString();
            var isTaskRun = exprStr.Contains("Task.Run") || exprStr.Contains("Task.Factory.StartNew");
            var isContinueWith = exprStr.Contains(".ContinueWith");

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

    private static void DetectServiceScopeFactory(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
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

    private static void DetectNewOutsideDI(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var objCreation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
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

    private static void DetectCancellationTokenNone(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        var severity = ExtractorHelpers.IsTestFile(filePath) ? "low" : "medium";

        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (memberAccess.ToString().EndsWith("CancellationToken.None"))
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

        // default(CancellationToken) — semantically equivalent to None
        foreach (var defaultExpr in root.DescendantNodes().OfType<DefaultExpressionSyntax>())
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

        // new CancellationToken() — default value, same as None
        foreach (var objCreation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
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

    private static void DetectUnboundedCollections(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        if (ExtractorHelpers.IsTestFile(filePath)) return;

        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            var fieldType = field.Declaration.Type.ToString();
            if ((fieldType.Contains("ConcurrentDictionary") ||
                 fieldType.Contains("ConcurrentBag") ||
                 fieldType.Contains("ConcurrentQueue"))
                && !fieldType.Contains("Channel"))
            {
                // Check if there's any cleanup/prune/clear method on this type
                var className = field.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText;
                var fullClass = className ?? "unknown";
                var hasCleanup = field.Ancestors().OfType<TypeDeclarationSyntax>()
                    .SelectMany(t => t.Members.OfType<MethodDeclarationSyntax>())
                    .Any(m => m.Identifier.ValueText.Contains("Clear", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Cleanup", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Prune", StringComparison.OrdinalIgnoreCase)
                           || m.Identifier.ValueText.Contains("Evict", StringComparison.OrdinalIgnoreCase));

                var line = field.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var severity = hasCleanup ? "low" : "medium";
                var note = hasCleanup ? " (cleanup method found)" : " — no eviction/cleanup method found";

                foreach (var variable in field.Declaration.Variables)
                {
                    model.Detections.Add(new AntiPatternDetection(
                        "UnboundedCollection",
                        $"`{fieldType} {variable.Identifier.ValueText}` in `{fullClass}`{note}.",
                        severity, variable.Identifier.ValueText)
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
        return lower.Contains("service") || lower.Contains("handler") || lower.Contains("manager")
            || lower.Contains("orchestrator") || lower.Contains("worker") || lower.Contains("runner")
            || lower.Contains("provider") || lower.Contains("resolver") || lower.Contains("factory")
            || lower.Contains("repository") || lower.Contains("dispatcher") || lower.Contains("tracker")
            || lower.Contains("adapter") || lower.Contains("broker");
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

    private static void DetectAsyncVoid(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (!method.Modifiers.Any(SyntaxKind.AsyncKeyword))
                continue;

            if (method.ReturnType is not PredefinedTypeSyntax rt
                || !rt.Keyword.IsKind(SyntaxKind.VoidKeyword))
                continue;

            // Skip event handler signatures: (object sender, XxxEventArgs e)
            if (method.ParameterList.Parameters.Count >= 2
                && method.ParameterList.Parameters[1].Type?.ToString().Contains("EventArgs") == true)
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
                && !string.IsNullOrEmpty(di.ServiceType) && di.ServiceType != "?"
                && !di.ServiceType.StartsWith("Add"))
            {
                var svc = di.ServiceType.Trim();
                lifetimeMap[svc] = di.Lifetime;
            }
        }

        // Check types for CaptiveDependency: Singleton → Scoped dependency
        foreach (var type in model.Types.Values)
        {
            var ctors = type.Methods.Where(m => m.Name == ".ctor" || m.Name == type.Name).ToList();
            if (ctors.Count == 0) continue;

            foreach (var paramType in ctors[0].ParameterTypes)
            {
                var trimmed = paramType.Trim();
                if (lifetimeMap.TryGetValue(trimmed, out var depLifetime)
                    && depLifetime == "Scoped"
                    && lifetimeMap.TryGetValue(type.Name, out var selfLifetime)
                    && selfLifetime == "Singleton")
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
