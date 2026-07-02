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

        var depCounts = new Dictionary<string, int>();
        foreach (var proj in model.Projects)
        {
            foreach (var dep in proj.ProjectReferences)
            {
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
