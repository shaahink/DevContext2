using DevContext.Core.Graph2;

namespace DevContext.Core.Graph;

/// <summary>Builds CLI command entry points from <see cref="CliCommandDetection"/>s.</summary>
public sealed class CliCommandEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        SymbolTable names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cmd in model.Detections.OfType<CliCommandDetection>())
        {
            if (!scope.Contains(cmd.SourceFile) || !noise.IsProductionEntrySource(cmd.SourceFile)) continue;
            if (!seen.Add(cmd.CommandType)) continue;

            // B4 (D1.1d): plain-Main fallback entries carry no settings type — title is the exe itself.
            // Batch B: a declared verb leads, because that is what the user types.
            var title = cmd.CommandName is { Length: > 0 } verb
                ? $"{verb} ({cmd.CommandType})"
                : cmd.SettingsType.Length > 0
                    ? $"{cmd.CommandType} —settings {cmd.SettingsType}"
                    : $"{cmd.CommandType} (Main)";
            var id = NodeId.ForEntry($"cli:{cmd.CommandType}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = cmd.SourceFile, LineNumber = cmd.LineNumber });

            var typeId = NodeId.ForType(names.ResolveName(cmd.CommandType, cmd.SourceFile));
            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, typeId, EdgeKind.Calls)
                {
                    Provenance = $"{cmd.SourceFile}:{cmd.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.CliCommand, title, id)
            {
                Provenance = $"{cmd.SourceFile}:{cmd.LineNumber}",
                HandlerNode = typeId,
                Project = scope.ProjectForFile(cmd.SourceFile),
            });
        }
        return entries.ToImmutable();
    }
}
