namespace DevContext.Core.Configuration;

/// <summary>Infers the most likely scenario and extraction profile from a user's task description using keyword matching.</summary>
public static class IntentInferrer
{
    private static readonly (string[] Keywords, string Scenario, ExtractionProfile Profile)[] Rules =
    [
        (["debug", "why", "failing", "error", "exception", "500"], "debug-endpoint", ExtractionProfile.Debug),
        (["add", "implement", "similar", "like", "crud", "new endpoint"], "add-similar-feature", ExtractionProfile.Focused),
        (["middleware", "pipeline", "cross-cutting", "filter", "interceptor"], "modify-middleware", ExtractionProfile.Focused),
        (["event", "message", "publish", "consume", "queue", "bus"], "trace-message-flow", ExtractionProfile.Focused),
        (["architecture", "overview", "structure", "layers", "map"], "architecture", ExtractionProfile.Focused),
        (["di", "injection", "reflect", "activator", "register"], "harden-di", ExtractionProfile.Debug),
    ];

    /// <summary>Infers the scenario and profile from a task description by matching keywords.</summary>
    public static (string Scenario, ExtractionProfile Profile) Infer(string task)
    {
        var lower = task.ToLowerInvariant();
        var best = Rules
            .Select(r => (r.Scenario, r.Profile, Score: r.Keywords.Count(k => lower.Contains(k))))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return best.Score > 0 ? (best.Scenario, best.Profile) : ("architecture", ExtractionProfile.Focused);
    }
}
