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

        // 1. Aliases
        var scenarioKey = input.ExplicitScenario;
        if (scenarioKey == "trace")
            scenarioKey = "deep-dive";
        if (scenarioKey == "audit")
        {
            warnings.Add("'audit' is deprecated. Use 'overview' instead.");
            scenarioKey = "overview";
        }
        if (scenarioKey is not null && !ScenarioRegistry.BuiltIn.ContainsKey(scenarioKey))
            throw new ArgumentException($"Unknown scenario: {scenarioKey}");

        // 2. Scenario: explicit → deep-dive if focus present → overview
        var hasFocus = !string.IsNullOrWhiteSpace(input.Focus);
        var finalScenarioKey = scenarioKey ?? (hasFocus ? "deep-dive" : "overview");
        var scenario = ScenarioRegistry.BuiltIn[finalScenarioKey];

        // 3. Depth overrides
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

        // 4. Profile: explicit → derived from scenario
        ExtractionProfile profile;
        if (input.ExplicitProfile is { } ep)
        {
            profile = ep.ToLowerInvariant() switch
            {
                "debug" => ExtractionProfile.Debug,
                "full" => ExtractionProfile.Full,
                _ => ExtractionProfile.Focused,
            };
        }
        else
        {
            profile = finalScenarioKey == "deep-dive" ? ExtractionProfile.Debug : ExtractionProfile.Focused;
        }

        // 5. Focus parsing
        var focusPoints = ImmutableArray.CreateBuilder<FocusPoint>();
        if (hasFocus)
        {
            var text = input.Focus!.Trim();

            // Endpoint route detection: contains '/' or starts with HTTP verb
            if (text.Contains('/') || IsHttpVerbPrefixed(text))
            {
                var (verb, route) = ParseEndpointFocus(text);
                focusPoints.Add(new FocusPoint(FocusKind.Endpoint, "", null, null,
                    HttpMethod: verb, Route: route));
            }
            else
            {
                // Parse Type or Type:Method (no filesystem needed here)
                var fp = ParseTypeOrMethodFocus(text);
                if (fp is not null)
                    focusPoints.Add(fp);
            }
        }

        // 6. Explanation
        string explanation;
        if (!hasFocus)
        {
            explanation = "Overview map (no focus).";
        }
        else if (focusPoints.Count > 0 && focusPoints[0].Kind == FocusKind.Endpoint)
        {
            var fp = focusPoints[0];
            explanation = $"Slicing from {fp.HttpMethod} {fp.Route} — handler resolved after scan.";
        }
        else
        {
            var name = input.Focus!;
            var depthInfo = input.Depth is { } d ? $", depth {d}" : "";
            explanation = $"Slicing from {name}{depthInfo}, call graph on.";
        }

        // Warn when deep-dive has no focus
        if (finalScenarioKey == "deep-dive" && !hasFocus && input.ExplicitScenario is not null)
        {
            warnings.Add("deep-dive without --focus behaves like overview — give a starting point");
        }

        return new ResolvedIntent
        {
            Scenario = scenario,
            Profile = profile,
            FocusPoints = focusPoints.ToImmutable(),
            Explanation = explanation,
            Warnings = warnings.ToImmutable(),
        };
    }

    private static bool IsHttpVerbPrefixed(string text)
    {
        var upper = text.ToUpperInvariant();
        return upper.StartsWith("GET ") || upper.StartsWith("POST ") || upper.StartsWith("PUT ")
            || upper.StartsWith("PATCH ") || upper.StartsWith("DELETE ") || upper.StartsWith("HEAD ")
            || upper.StartsWith("OPTIONS ");
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
