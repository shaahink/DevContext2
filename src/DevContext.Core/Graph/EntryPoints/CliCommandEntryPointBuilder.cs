namespace DevContext.Core.Graph;

/// <summary>Builds CLI command entry points from <see cref="CliCommandDetection"/>s.</summary>
public sealed class CliCommandEntryPointBuilder : IEntryPointBuilder
{
    public ImmutableArray<EntryPoint> Build(
        CodeGraphBuilder g, DiscoveryModel model, SolutionScope scope,
        NameResolver names, NoiseFilter noise)
    {
        var entries = ImmutableArray.CreateBuilder<EntryPoint>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cmd in model.Detections.OfType<CliCommandDetection>())
        {
            if (!scope.Contains(cmd.SourceFile) || !noise.IsProductionEntrySource(cmd.SourceFile)) continue;
            if (!seen.Add(cmd.CommandType)) continue;

            var title = $"{cmd.CommandType} —settings {cmd.SettingsType}";
            var id = NodeId.ForEntry($"cli:{cmd.CommandType}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = cmd.SourceFile });

            var typeId = NodeId.ForType(names.Resolve(cmd.CommandType));
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
            });
        }
        return entries.ToImmutable();
    }
}
