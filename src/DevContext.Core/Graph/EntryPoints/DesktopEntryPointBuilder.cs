namespace DevContext.Core.Graph;

/// <summary>Builds desktop UI entry points (Window, Page, UserControl, AppStartup, RelayCommand)
/// from <see cref="DesktopEntryDetection"/>s. W5: WinUI/WPF/Avalonia/MAUI desktop apps.</summary>
public sealed class DesktopEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var de in model.Detections.OfType<DesktopEntryDetection>())
        {
            if (!scope.Contains(de.SourceFile) || !noise.IsProductionEntrySource(de.SourceFile)) continue;

            var label = de.Kind == DesktopEntryKind.RelayCommand
                ? de.TypeName
                : de.TypeName;

            if (!seen.Add(label)) continue;

            var id = NodeId.ForEntry($"ui:{de.TypeName}");
            var title = de.Kind == DesktopEntryKind.RelayCommand
                ? $"[RelayCommand] {de.TypeName}"
                : de.TypeName;

            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = de.SourceFile });

            var typeName = de.Kind == DesktopEntryKind.RelayCommand
                ? (de.TypeName.Contains('.') ? de.TypeName[..de.TypeName.LastIndexOf('.')] : de.TypeName)
                : de.TypeName;
            var handlerNodeId = g.HasNode(NodeId.ForType(names.Resolve(typeName)))
                ? NodeId.ForType(names.Resolve(typeName))
                : (NodeId?)null;

            if (handlerNodeId is { } hn)
                g.AddEdge(new GraphEdge(id, hn, EdgeKind.Calls)
                {
                    Provenance = $"{de.SourceFile}:{de.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.UiEntry, title, id)
            {
                Provenance = $"{de.SourceFile}:{de.LineNumber}",
                HandlerNode = handlerNodeId,
            });
        }
        return entries.ToImmutable();
    }
}
