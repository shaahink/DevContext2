namespace DevContext.Core.Configuration;

/// <summary>User's expressed intent — the unified Focus + Depth model.</summary>
public sealed record IntentInput
{
    public string? Focus { get; init; }
    public int? Depth { get; init; }
    public string? ExplicitScenario { get; init; }
    public string? ExplicitProfile { get; init; }
}

/// <summary>Fully resolved intent — scenario, profile, focus points, and an explanation string.</summary>
public sealed record ResolvedIntent
{
    public required Scenario Scenario { get; init; }
    public required ExtractionProfile Profile { get; init; }
    public required ImmutableArray<FocusPoint> FocusPoints { get; init; }
    public required string Explanation { get; init; }
    public ImmutableArray<string> Warnings { get; init; } = [];
}

/// <summary>Resolves a user's Focus + Depth into a scenario, profile, and focus points.
/// One derivation, shared by CLI and Desktop.</summary>
public static class AnalysisIntentResolver
{
    public static ResolvedIntent Resolve(IntentInput input)
    {
        var warnings = ImmutableArray.CreateBuilder<string>();

        var (finalScenarioKey, scenario) = ResolveScenario(input, warnings);
        ExtractionProfile profile = ResolveProfile(input, finalScenarioKey);
        var focusPoints = ParseFocusPoints(input);
        var explanation = BuildExplanation(input, focusPoints);

        if (string.Equals(finalScenarioKey, "deep-dive", StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(input.Focus)
            && input.ExplicitScenario is not null)
        {
            warnings.Add("deep-dive without --focus behaves like overview — give a starting point");
        }

        return new ResolvedIntent
        {
            Scenario = scenario,
            Profile = profile,
            FocusPoints = focusPoints,
            Explanation = explanation,
            Warnings = warnings.ToImmutable(),
        };
    }

    private static (string Key, Scenario Scenario) ResolveScenario(IntentInput input, ImmutableArray<string>.Builder warnings)
    {
        var scenarioKey = input.ExplicitScenario;
        if (string.Equals(scenarioKey, "trace", StringComparison.Ordinal))
            scenarioKey = "deep-dive";
        if (string.Equals(scenarioKey, "audit", StringComparison.Ordinal))
        {
            warnings.Add("'audit' is deprecated. Use 'overview' instead.");
            scenarioKey = "overview";
        }
        if (scenarioKey is not null && !ScenarioRegistry.BuiltIn.ContainsKey(scenarioKey))
            throw new ArgumentException($"Unknown scenario: {scenarioKey}", nameof(input));

        var hasFocus = !string.IsNullOrWhiteSpace(input.Focus);
        var finalKey = scenarioKey ?? (hasFocus ? "deep-dive" : "overview");
        var scenario = ScenarioRegistry.BuiltIn[finalKey];

        if (input.Depth is { } depth)
        {
            var clamped = Math.Clamp(depth, 1, 10);
            scenario = scenario with
            {
                Pruning = scenario.Pruning with
                {
                    MaxCallDepth = clamped,
                    MaxPathDistance = clamped <= 2 ? 1 : 2,
                },
            };
        }

        return (finalKey, scenario);
    }

    private static ExtractionProfile ResolveProfile(IntentInput input, string finalScenarioKey)
    {
        if (input.ExplicitProfile is { } ep)
        {
            return ep.ToLowerInvariant() switch
            {
                "debug" => ExtractionProfile.Debug,
                "full" => ExtractionProfile.Full,
                _ => ExtractionProfile.Focused,
            };
        }

        return string.Equals(finalScenarioKey, "deep-dive", StringComparison.Ordinal)
            ? ExtractionProfile.Debug
            : ExtractionProfile.Focused;
    }

    private static ImmutableArray<FocusPoint> ParseFocusPoints(IntentInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Focus))
            return [];

        var focusPoints = ImmutableArray.CreateBuilder<FocusPoint>();
        var text = input.Focus!.Trim();

        if (text.Contains('/') || IsHttpVerbPrefixed(text))
        {
            var (verb, route) = ParseEndpointFocus(text);
            focusPoints.Add(new FocusPoint(FocusKind.Endpoint, "", null, null,
                HttpMethod: verb, Route: route));
        }
        else
        {
            var fp = ParseTypeOrMethodFocus(text);
            if (fp is not null)
                focusPoints.Add(fp);
        }

        return focusPoints.ToImmutable();
    }

    private static string BuildExplanation(IntentInput input, ImmutableArray<FocusPoint> focusPoints)
    {
        if (string.IsNullOrWhiteSpace(input.Focus))
            return "Overview map (no focus).";

        if (focusPoints.Length > 0 && focusPoints[0].Kind == FocusKind.Endpoint)
        {
            var fp = focusPoints[0];
            return $"Slicing from {fp.HttpMethod} {fp.Route} — handler resolved after scan.";
        }

        var depthInfo = input.Depth is { } d ? $", depth {d}" : "";
        return $"Slicing from {input.Focus}{depthInfo}, call graph on.";
    }

    private static bool IsHttpVerbPrefixed(string text)
    {
        var upper = text.ToUpperInvariant();
        return upper.StartsWith("GET ", StringComparison.Ordinal) || upper.StartsWith("POST ", StringComparison.Ordinal) || upper.StartsWith("PUT ", StringComparison.Ordinal)
            || upper.StartsWith("PATCH ", StringComparison.Ordinal) || upper.StartsWith("DELETE ", StringComparison.Ordinal) || upper.StartsWith("HEAD ", StringComparison.Ordinal)
            || upper.StartsWith("OPTIONS ", StringComparison.Ordinal);
    }

    private static FocusPoint? ParseTypeOrMethodFocus(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        // Type:Method pattern (no path separators)
        var colonParts = input.Split(':');
        if (colonParts.Length == 2 && !input.Contains('\\') && !input.Contains('/'))
        {
            return new FocusPoint(FocusKind.Method, "", colonParts[0], colonParts[1]);
        }

        // Bare type name (no path separators)
        if (!input.Contains('\\') && !input.Contains('/'))
        {
            return new FocusPoint(FocusKind.Type, "", input, null);
        }

        return null;
    }

    private static (string? HttpMethod, string Route) ParseEndpointFocus(string text)
    {
        var trimmed = text.Trim();

        string? method = null;
        string route;
        if (IsHttpVerbPrefixed(trimmed))
        {
            var space = trimmed.IndexOf(' ');
            method = trimmed[..space].ToUpperInvariant();
            route = trimmed[(space + 1)..].Trim();
        }
        else
        {
            route = trimmed;
        }

        // Normalize route: ensure leading /
        if (!route.StartsWith('/'))
            route = "/" + route;

        return (method, route);
    }
}
