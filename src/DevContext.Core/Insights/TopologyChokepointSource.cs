using System.Collections.Immutable;

using DevContext.Core.Graph;
using DevContext.Core.Models;

namespace DevContext.Core.Insights;

public sealed class TopologyChokepointSource : IInsightSource
{
    public string Id => "topology.chokepoint";
    public InsightCategory Category => InsightCategory.Topology;

    public IEnumerable<Insight> Compute(DiscoveryModel model, CodeGraph graph, ImmutableArray<EntryPoint> entries)
    {
        if (model.Projects.Length < 3) yield break;

        // T1.9 — most-depended-upon names a PRODUCTION project. A sample/test/benchmark project must never
        // be the answer (MediatR.Examples outranking MediatR), but it still counts as a dependent — for a
        // library repo its samples/tests ARE what depends on the library, so excluding them as the answer
        // (not as dependents) is what surfaces MediatR itself. Classify by project, not path regex.
        var projByName = new Dictionary<string, ProjectInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in model.Projects)
            projByName[ProjectName(p.FilePath)] = p;

        var depCounts = new Dictionary<string, int>();
        foreach (var proj in model.Projects)
        {
            foreach (var dep in proj.ProjectReferences)
            {
                // The depended project (the answer) must be production — skip MediatR.Examples, benchmarks…
                if (projByName.TryGetValue(ProjectName(dep), out var depProj)
                    && !ProjectClassifier.IsProductionProject(depProj))
                    continue;
                if (!depCounts.ContainsKey(dep)) depCounts[dep] = 0;
                depCounts[dep]++;
            }
        }

        var top = depCounts
            .OrderByDescending(kv => kv.Value)
            .Take(3)
            .Select(kv => $"{ProjectName(kv.Key)} ({kv.Value} dependents)")
            .ToList();

        if (top.Count == 0) yield break;

        var severity = top[0].Contains(top[0]) && depCounts.Values.First() >= 3 ? Severity.Notable : Severity.Info;
        yield return Insight.Create(Id, Category, severity,
            $"Most depended-upon: {string.Join(" · ", top)}", top);
    }

    private static string ProjectName(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return name.EndsWith(".csproj") ? Path.GetFileNameWithoutExtension(name) : name;
    }
}
