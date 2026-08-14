using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds desktop UI entry points (Window, Page, UserControl, AppStartup, RelayCommand)
/// from <see cref="DesktopEntryDetection"/>s. W5: WinUI/WPF/Avalonia/MAUI desktop apps.
/// <para>C2 (Prism D2): entries link at MEMBER level so the trace descends instead of dead-ending
/// on the Type node (the audit's "ExportPanel → ExportPanel, 3 lines"). A RelayCommand entry links
/// to its exact command member; a Window/Page/UserControl links to its constructor (the setup
/// wiring) and its event-handler-shaped members (<c>(object sender, …EventArgs e)</c> — the
/// signature IS the XAML wiring convention, so no XAML parse is needed).</para></summary>
public sealed class DesktopEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
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

            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = de.SourceFile, LineNumber = de.LineNumber });

            var isCommand = de.Kind == DesktopEntryKind.RelayCommand && de.TypeName.Contains('.');
            var typeName = isCommand ? de.TypeName[..de.TypeName.LastIndexOf('.')] : de.TypeName;
            var typeFqn = names.ResolveName(typeName, de.SourceFile);
            var typeNodeId = g.HasNode(NodeId.ForType(typeFqn)) ? NodeId.ForType(typeFqn) : (NodeId?)null;

            NodeId? handlerNodeId = null;
            if (typeNodeId is not null)
            {
                foreach (var member in EntryMembers(de, isCommand, typeFqn, model))
                {
                    var memberId = NodeId.ForMember(typeFqn, member);
                    g.AddNode(new GraphNode(memberId, SymbolCanon.MemberTitle(memberId.Key), NodeKind.Member)
                    {
                        FilePath = de.SourceFile,
                    });
                    g.AddEdge(new GraphEdge(id, memberId, EdgeKind.Calls)
                    {
                        Provenance = $"{de.SourceFile}:{de.LineNumber}",
                        Resolution = Resolution.Join,
                    });
                    handlerNodeId ??= memberId; // enumeration order = priority (command/ctor first)
                }
            }

            // No member evidence (e.g. a marker window with no ctor/handlers in scan scope) —
            // keep the pre-C2 type-node link so the owning-type fallback target still works.
            handlerNodeId ??= typeNodeId;
            if (handlerNodeId == typeNodeId && typeNodeId is { } tn)
                g.AddEdge(new GraphEdge(id, tn, EdgeKind.Calls)
                {
                    Provenance = $"{de.SourceFile}:{de.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.UiEntry, title, id)
            {
                Provenance = $"{de.SourceFile}:{de.LineNumber}",
                HandlerNode = handlerNodeId,
                Project = scope.ProjectForFile(de.SourceFile),
            });
        }
        return entries.ToImmutable();
    }

    /// <summary>The members a desktop entry dispatches into, in priority order: the exact command
    /// member for a RelayCommand; else the view's constructor followed by its event-handler-shaped
    /// methods (exactly two parameters, <c>object</c> first, <c>…EventArgs</c>-ish second).</summary>
    private static IEnumerable<string> EntryMembers(
        DesktopEntryDetection de, bool isCommand, string typeFqn, DiscoveryModel model)
    {
        if (isCommand)
        {
            yield return de.TypeName[(de.TypeName.LastIndexOf('.') + 1)..];
            yield break;
        }

        if (!model.Types.TryGetValue(typeFqn, out var type)) yield break;

        var shortName = typeFqn[(typeFqn.LastIndexOf('.') + 1)..];
        var yielded = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in type.Methods)
            if (m.Name == shortName && m.ReturnType == "ctor" && yielded.Add(m.Name))
                yield return m.Name;

        foreach (var m in type.Methods)
        {
            if (m.ParameterTypes.Length != 2) continue;
            if (!string.Equals(m.ParameterTypes[0], "object", StringComparison.Ordinal)) continue;
            var second = m.ParameterTypes[1];
            if (!second.EndsWith("EventArgs", StringComparison.Ordinal)
                && !second.EndsWith("EventArgs?", StringComparison.Ordinal)) continue;
            if (yielded.Add(m.Name)) yield return m.Name;
        }
    }
}
