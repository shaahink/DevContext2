using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DevContext.Core.Extractors.Specific;

/// <summary>Detects desktop UI entry points — Window/Page/UserControl subclasses, App.OnLaunched/
/// OnStartup, and [RelayCommand] handlers — for WinUI/WPF/Avalonia apps.</summary>
[ExtractorOrder(80)]
public sealed class DesktopEntryExtractor : IDiscoveryExtractor
{
    /// <summary>Gets the name of this extractor.</summary>
    public string Name => "DesktopEntryExtractor";
    /// <summary>Gets the execution tier.</summary>
    public ExtractorTier Tier => ExtractorTier.Fast;
    /// <summary>Gets the extractor category.</summary>
    public ExtractorCategory Category => ExtractorCategory.Specific;
    /// <summary>Gets the execution stage.</summary>
    public ExecutionStage Stage => ExecutionStage.Stage3Specific;
    /// <summary>Describes the signals and model fields this extractor uses.</summary>
    public ExtractorCapabilities Capabilities => new(
        [ArchitectureSignals.Keys.DesktopUi],
        ["desktop-entry-detections"],
        ["model.Detections"],
        "Scans syntax trees for Window/Page/UserControl subclasses, App.OnLaunched/OnStartup, and [RelayCommand] handlers");
    /// <summary>Only runs when the desktop-ui signal has been detected.</summary>
    public bool ShouldRun(DiscoveryContext context, DiscoveryModel currentModel)
        => currentModel.Architecture.Has(ArchitectureSignals.Keys.DesktopUi);

    public async ValueTask ExtractAsync(DiscoveryContext context, DiscoveryModel model, CancellationToken ct)
    {
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
                continue;
            }

            var root = await syntaxTree.GetRootAsync(ct);

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ct.ThrowIfCancellationRequested();
                var className = classDecl.Identifier.ValueText;
                if (string.IsNullOrEmpty(className)) continue;

                // App classes with OnLaunched / OnStartup
                if (className == "App")
                {
                    foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                    {
                        var methodName = method.Identifier.ValueText;
                        if (methodName is "OnLaunched" or "OnStartup" or "OnActivated"
                            or "OnBackgroundActivated" or "OnFileActivated")
                        {
                            model.Detections.Add(new DesktopEntryDetection(className, DesktopEntryKind.AppStartup)
                            {
                                ExtractorName = Name,
                                SourceFile = filePath,
                                LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                            });
                            break;
                        }
                    }
                }

                // Window / Page / UserControl subclasses (WinUI, WPF, Avalonia, MAUI)
                // plus Form (WinForms) and ContentPage (MAUI/Xamarin)
                var baseType = GetBaseTypeName(classDecl);
                if (baseType is "Window" or "Page" or "UserControl" or "ContentDialog"
                    or "Form" or "ContentPage" or "Shell" or "Application" && className != "App")
                {
                    model.Detections.Add(new DesktopEntryDetection(className, MapBaseToKind(baseType))
                    {
                        ExtractorName = Name,
                        SourceFile = filePath,
                        LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    });
                }

                // [RelayCommand] methods
                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    if (HasAttribute(method, "RelayCommand"))
                    {
                        model.Detections.Add(new DesktopEntryDetection(
                            $"{className}.{method.Identifier.ValueText}", DesktopEntryKind.RelayCommand)
                        {
                            ExtractorName = Name,
                            SourceFile = filePath,
                            LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        });
                    }
                }
            }
        }
    }

    private static string? GetBaseTypeName(ClassDeclarationSyntax classDecl)
    {
        var baseList = classDecl.BaseList;
        if (baseList is null) return null;
        foreach (var bt in baseList.Types)
        {
            var name = bt.Type.ToString();
            if (name.Contains('<')) name = name[..name.IndexOf('<')];
            // Return the last segment (simple name)
            var dot = name.LastIndexOf('.');
            var simple = dot >= 0 ? name[(dot + 1)..] : name;
            if (simple is "Window" or "Page" or "UserControl" or "ContentDialog" or "Application"
                or "Form" or "ContentPage" or "Shell")
                return simple;
        }
        return null;
    }

    private static bool HasAttribute(MethodDeclarationSyntax method, string attributeName)
    {
        foreach (var attrList in method.AttributeLists)
        {
            foreach (var attr in attrList.Attributes)
            {
                var name = attr.Name.ToString();
                if (name.Contains('<')) name = name[..name.IndexOf('<')];
                var dot = name.LastIndexOf('.');
                var simple = dot >= 0 ? name[(dot + 1)..] : name;
                if (string.Equals(simple, attributeName, StringComparison.Ordinal))
                    return true;
            }
        }
        return false;
    }

    private static DesktopEntryKind MapBaseToKind(string? baseType) => baseType switch
    {
        "Window" => DesktopEntryKind.Window,
        "Page" => DesktopEntryKind.Page,
        "UserControl" => DesktopEntryKind.UserControl,
        "ContentDialog" => DesktopEntryKind.Page,
        "Form" => DesktopEntryKind.Window,
        "ContentPage" => DesktopEntryKind.Page,
        "Shell" => DesktopEntryKind.Page,
        "Application" => DesktopEntryKind.AppStartup,
        _ => DesktopEntryKind.Window,
    };
}
