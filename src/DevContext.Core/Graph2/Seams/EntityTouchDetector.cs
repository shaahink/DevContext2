using DevContext.Core.Graph;

namespace DevContext.Core.Graph2.Seams;

/// <summary>Detects data access on the spine: a member that references a known entity/store short name
/// (via a receiver type, an object creation, or a whole-word identifier use). This replaces the old
/// hand-rolled <c>ContainsWholeWord</c> body scan — now trivially exact because identifiers are already
/// tokenised into <see cref="IdentifierUseOp"/>s. Emits <see cref="EdgeKind.ReadsWrites"/> once per
/// touched entity/store.</summary>
public sealed class EntityTouchDetector : ISeamDetector
{
    public string Id => "EntityTouch";

    public IEnumerable<SeamMatch> Detect(BodyFacts body, SeamContext ctx)
    {
        if (ctx.KnownEntities.IsEmpty) yield break;

        var emitted = new HashSet<string>(StringComparer.Ordinal);

        foreach (var op in body.Ops)
        {
            string? name = op switch
            {
                InvocationOp inv when inv.ReceiverType is { } rt && ctx.KnownEntities.Contains(rt.Text) => rt.Text,
                CreationOp c when ctx.KnownEntities.Contains(c.Type.Text) => c.Type.Text,
                IdentifierUseOp u when ctx.KnownEntities.Contains(u.Identifier) => u.Identifier,
                _ => null,
            };
            if (name is null || !emitted.Add(name)) continue;

            var line = op.Line;
            var target = new SymbolRef
            {
                Text = name,
                Site = new RefSite { File = body.File, Line = line, Project = body.Project },
            };
            yield return new SeamMatch(
                body.Member, EdgeKind.ReadsWrites, SeamDetectorHelpers.Resolve(target, ctx),
                0.5f, $"{body.File}:{line}", Id);
        }
    }
}
