namespace DevContext.Core.Configuration;

/// <summary>Infers the most likely scenario and extraction profile from a user's task description using keyword matching.</summary>
public static class IntentInferrer
{
    private static readonly (string[] Keywords, string Scenario, ExtractionProfile Profile)[] Rules =
    [
        (["debug", "why", "failing", "error", "exception", "500", "trace", "call graph"], "deep-dive", ExtractionProfile.Debug),
        (["add", "implement", "similar", "like", "crud", "new endpoint", "architecture", "overview", "structure", "layers", "map"], "overview", ExtractionProfile.Focused),
        (["di", "injection", "reflect", "activator", "register", "middleware", "pipeline", "audit", "wiring"], "audit", ExtractionProfile.Debug),
        (["event", "message", "publish", "consume", "queue", "bus"], "deep-dive", ExtractionProfile.Focused),
    ];

    public static (string Scenario, ExtractionProfile Profile) Infer(string task)
    {
        var lower = task.ToLowerInvariant();
        var best = Rules
            .Select(r => (r.Scenario, r.Profile, Score: r.Keywords.Count(k => lower.Contains(k))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return best.Score > 0 ? (best.Scenario, best.Profile) : ("overview", ExtractionProfile.Focused);
    }
}
