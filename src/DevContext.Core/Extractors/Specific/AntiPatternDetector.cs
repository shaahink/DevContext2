using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects common anti-patterns: fire-and-forget tasks, IServiceScopeFactory, new outside DI, CancellationToken.None, unbounded collections.</summary>
[ExtractorOrder(45)]
public sealed class AntiPatternDetector : IDiscoveryExtractor
{
    public string Name => "AntiPatternDetector";
    public ExtractorTier Tier => ExtractorTier.Fast;
    public ExtractorCategory Category => ExtractorCategory.Specific;
    public ExecutionStage Stage => ExecutionStage.Stage3Sequential;
    public ExtractorCapabilities Capabilities => new(
        [], [],
        ["model.Detections"],
        "Detects fire-and-forget tasks, IServiceScopeFactory, new outside DI, CancellationToken.None, unbounded collections");

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
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct).ConfigureAwait(false);

            DetectFireAndForget(root, filePath, model);
            DetectServiceScopeFactory(root, filePath, model);
            DetectNewOutsideDI(root, filePath, model);
            DetectCancellationTokenNone(root, filePath, model);
            DetectUnboundedCollections(root, filePath, model);
        }
    }

    private static void DetectFireAndForget(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            // Pattern: _ = SomeAsyncMethod(...)
            if (assignment.Left.ToString() == "_" &&
                assignment.Right is InvocationExpressionSyntax inv)
            {
                var methodName = GetMethodName(inv);
                var line = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "FireAndForget",
                    $"Discard assignment to `{methodName}` — task is never awaited. Exceptions may be lost.",
                    "high", methodName)
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }

        // Pattern: _ = Task.Run(...) — fire-and-forget task
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression.ToString().Contains(".ContinueWith") &&
                inv.Parent is ExpressionStatementSyntax &&
                inv.Parent.Parent is BlockSyntax)
            {
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "FireAndForget",
                    "ContinueWith without await — unobserved task continuation.",
                    "medium", "ContinueWith")
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
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var expr = inv.Expression.ToString();
            if (expr.Contains("CreateScope") || expr.Contains("_scopeFactory") || expr.Contains("scopeFactory"))
            {
                var line = inv.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "ServiceLocator",
                    $"IServiceScopeFactory usage: `{Truncate(expr, 80)}` — manual service location. Prefer constructor injection.",
                    "high", "IServiceScopeFactory")
                {
                    ExtractorName = "AntiPatternDetector",
                    SourceFile = filePath,
                    LineNumber = line
                });
            }
        }
    }

    private static void DetectNewOutsideDI(SyntaxNode root, string filePath, DiscoveryModel model)
    {
        foreach (var objCreation in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            // Skip if inside a constructor, field initializer, or DI registration (lambdas)
            if (IsInConstructorOrDI(objCreation)) continue;

            var typeName = objCreation.Type.ToString();
            // Skip framework types, primitives, records in method bodies (they're often DTOs)
            if (typeName is "ArgumentNullException" or "InvalidOperationException" or "NotSupportedException"
                or "List" or "Dictionary" or "ConcurrentDictionary" or "StringBuilder"
                or "CancellationTokenSource" or "JsonSerializerOptions" or "DbContextOptionsBuilder"
                or "SqliteConnection")
                continue;

            // Only flag if the type name looks like a service/handler/manager
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
        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (memberAccess.ToString().EndsWith("CancellationToken.None"))
            {
                var line = memberAccess.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                model.Detections.Add(new AntiPatternDetection(
                    "CancellationTokenNone",
                    "`CancellationToken.None` used — operation cannot be cancelled.",
                    "medium", "CancellationToken.None")
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
        if (ancestor is ConstructorDeclarationSyntax or FieldDeclarationSyntax or PropertyDeclarationSyntax)
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

    private static async IAsyncEnumerable<string> EnumerateSourceFilesAsync(
        DiscoveryContext context, [EnumeratorCancellation] CancellationToken ct)
    {
        foreach (var file in context.Analysis.AllSourceFiles)
        {
            ct.ThrowIfCancellationRequested();
            yield return file;
        }
    }
}

/// <summary>Detection for an anti-pattern found in the codebase.</summary>
public sealed record AntiPatternDetection(
    string Pattern,
    string Description,
    string Severity,
    string TargetType
) : Detection;
