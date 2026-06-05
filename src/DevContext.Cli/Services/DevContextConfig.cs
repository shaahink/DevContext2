using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevContext.Cli.Services;

public sealed record DevContextConfig
{
    [JsonPropertyName("defaultProfile")]
    public string? DefaultProfile { get; init; }

    [JsonPropertyName("defaultScenario")]
    public string? DefaultScenario { get; init; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; init; }

    [JsonPropertyName("excludePatterns")]
    public ImmutableArray<string>? ExcludePatterns { get; init; }

    [JsonPropertyName("entryPaths")]
    public ImmutableArray<string>? EntryPaths { get; init; }

    [JsonPropertyName("profiles")]
    public Dictionary<string, ProfileConfig>? Profiles { get; init; }

    public static DevContextConfig? Load(string configPath)
    {
        if (!File.Exists(configPath)) return null;
        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<DevContextConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (MaxOutputTokens is < 100 or > 100000)
            errors.Add($"maxOutputTokens must be between 100 and 100000");
        if (DefaultProfile is not null and not ("quick" or "focused" or "debug" or "full"))
            errors.Add($"defaultProfile must be one of: quick, focused, debug, full");
        if (DefaultScenario is not null && !ScenarioRegistry.BuiltIn.ContainsKey(DefaultScenario))
            errors.Add($"defaultScenario '{DefaultScenario}' is not a registered scenario");
        return errors;
    }

    public static string DefaultPath => Path.Combine(Environment.CurrentDirectory, "devcontext.json");
}

public sealed record ProfileConfig
{
    [JsonPropertyName("profile")]
    public string? Profile { get; init; }

    [JsonPropertyName("maxOutputTokens")]
    public int? MaxOutputTokens { get; init; }

    [JsonPropertyName("noRoslyn")]
    public bool? NoRoslyn { get; init; }
}
