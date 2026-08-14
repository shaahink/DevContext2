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
        var byType = VerbsByCommandType(model);
        foreach (var cmd in model.Detections.OfType<CliCommandDetection>())
        {
            if (!scope.Contains(cmd.SourceFile) || !noise.IsProductionEntrySource(cmd.SourceFile)) continue;
            if (!seen.Add(cmd.CommandType)) continue;

            // B4 (D1.1d): plain-Main fallback entries carry no settings type — title is the exe itself.
            // Batch B: a declared verb leads, because that is what the user types.
            var title = VerbPath(cmd, byType) is { Length: > 0 } verb
                ? $"{verb} ({cmd.CommandType})"
                : cmd.SettingsType.Length > 0
                    ? $"{cmd.CommandType} —settings {cmd.SettingsType}"
                    : $"{cmd.CommandType} (Main)";
            var id = NodeId.ForEntry($"cli:{cmd.CommandType}");
            g.AddNode(new GraphNode(id, title, NodeKind.EntryPoint) { FilePath = cmd.SourceFile, LineNumber = cmd.LineNumber });

            var typeFqn = names.ResolveName(cmd.CommandType, cmd.SourceFile);
            var typeId = NodeId.ForType(typeFqn);

            // D-3: join the EXECUTE MEMBER, not just the type. The type join is a dead end by
            // construction — ResolveEntryTarget's Type arm reads only Sends edges, and the call edges a
            // verb actually makes hang off its execute member — so GitVersion's five verbs each had their
            // Calls[Join] edge, an existing target Type node, and no resolved target at all (0 of 5).
            // The member is identified by SHAPE, not by a method-name list: the one method this command
            // type declares that takes the detected SETTINGS type. Survives a rename of InvokeAsync, and
            // a type with no such method (or more than one) keeps the type join rather than guessing.
            var handlerId = typeId;
            if (g.HasNode(typeId) && ExecuteMemberOf(cmd, typeFqn, model, names) is { } executeMember)
            {
                handlerId = NodeId.ForMember(typeFqn, executeMember);
                // Entry builders run before AddCallEdges (GraphBuilder.Build), so the member node never
                // exists yet — the builder creates it, exactly as HttpEntryPointBuilder does for a
                // controller action.
                g.AddNode(new GraphNode(handlerId, SymbolCanon.MemberTitle(handlerId.Key), NodeKind.Member)
                {
                    FilePath = cmd.SourceFile,
                });
            }

            if (g.HasNode(typeId))
                g.AddEdge(new GraphEdge(id, handlerId, EdgeKind.Calls)
                {
                    Provenance = $"{cmd.SourceFile}:{cmd.LineNumber}",
                    Resolution = Resolution.Join,
                });

            entries.Add(new EntryPoint(EntryPointKind.CliCommand, title, id)
            {
                Provenance = $"{cmd.SourceFile}:{cmd.LineNumber}",
                HandlerNode = handlerId,
                Project = scope.ProjectForFile(cmd.SourceFile),
            });
        }
        return entries.ToImmutable();
    }

    /// <summary>D1.3 (#14) — every detected command type that declares a verb, so a sub-command can be
    /// resolved to the verb its PARENT declares. The map is built from the detections themselves: a
    /// parent lives in another project's file (GitVersion's ConfigCommand is in GitVersion.Core, its
    /// init/show children in GitVersion.Configuration), so nothing but the whole detection set knows
    /// both ends.</summary>
    private static Dictionary<string, CliCommandDetection> VerbsByCommandType(DiscoveryModel model)
    {
        var map = new Dictionary<string, CliCommandDetection>(StringComparer.Ordinal);
        foreach (var cmd in model.Detections.OfType<CliCommandDetection>())
        {
            if (cmd.CommandName is not { Length: > 0 }) continue;
            map.TryAdd(cmd.CommandType, cmd);
        }
        return map;
    }

    /// <summary>D1.3 (#14) — the verb a user actually types: <c>"config init"</c>, not <c>"init"</c>.
    /// Walks the parent chain the generic command attribute declares. An unresolvable parent (declared
    /// in a repo we cannot see, or carrying no verb of its own) contributes NOTHING rather than a
    /// guessed prefix — the leaf verb is still true. Cycles and runaway depth stop the walk.</summary>
    private static string VerbPath(CliCommandDetection cmd, Dictionary<string, CliCommandDetection> byType)
    {
        if (cmd.CommandName is not { Length: > 0 } leaf) return "";
        var prefixes = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal) { cmd.CommandType };
        var parent = cmd.ParentCommandType;
        while (parent is { Length: > 0 } && seen.Add(parent) && prefixes.Count < 8)
        {
            if (!byType.TryGetValue(parent, out var owner) || owner.CommandName is not { Length: > 0 } verb) break;
            prefixes.Insert(0, verb);
            parent = owner.ParentCommandType;
        }
        return prefixes.Count == 0 ? leaf : $"{string.Join(' ', prefixes)} {leaf}";
    }

    /// <summary>D-3 — the command's execute member, named by shape: the single method the command type
    /// declares whose parameter list carries the detected settings type. Two structural facts, no name
    /// list (Batch E's rule): the class is already a detected command, and the settings type must resolve
    /// to a type we actually declare — which is also what rejects <see cref="CliCommandDetection"/>'s
    /// <c>"object"</c> sentinel for a command with no settings generic at all. Null when nothing matches
    /// or more than one method does; the caller then keeps the type-level join.</summary>
    private static string? ExecuteMemberOf(
        CliCommandDetection cmd, string typeFqn, DiscoveryModel model, SymbolTable names)
    {
        if (cmd.SettingsType is not { Length: > 0 } settings) return null;
        var settingsFqn = names.ResolveName(settings, cmd.SourceFile);
        if (!names.IsKnownFqn(settingsFqn)) return null;
        if (!model.Types.TryGetValue(typeFqn, out var decl) || decl.Methods.IsDefaultOrEmpty) return null;

        string? found = null;
        foreach (var m in decl.Methods)
        {
            if (m.ParameterTypes.IsDefaultOrEmpty) continue;
            var takesSettings = false;
            foreach (var p in m.ParameterTypes)
            {
                if (!string.Equals(names.ResolveName(p.TrimEnd('?'), cmd.SourceFile), settingsFqn,
                        StringComparison.Ordinal)) continue;
                takesSettings = true;
                break;
            }
            if (!takesSettings) continue;
            if (found is not null) return null;      // ambiguous — never guess which one runs the verb
            found = m.Name;
        }
        return found;
    }
}
