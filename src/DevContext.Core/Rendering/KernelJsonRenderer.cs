using System.Text.Json;
using System.Text.Json.Serialization;

using DevContext.Core.Graph;
using DevContext.Core.Insights;

namespace DevContext.Core.Rendering;

/// <summary>Renders the analysis result as kernel JSON — the canonical wire format consumed by
/// CLI, desktop, MCP, and web. Replaces the legacy catalog-based <see cref="JsonContextRenderer"/>
/// (W9 retirement). Schema version "devcontext/v1".</summary>
public sealed class KernelJsonRenderer : IContextRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public string Format => "json";

    public ValueTask<RenderedContext> RenderAsync(DiscoveryModel model, RenderOptions options, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var snapshot = options.Snapshot;
        var output = new KernelJsonOutput
        {
            Schema = "devcontext/v1",
            GeneratedAt = DateTimeOffset.UtcNow,
            Archetype = model.Archetype ?? "App",
            ArchitectureStyle = model.DetectedStyle.ToString(),
            StyleConfidence = (float)Math.Round(model.StyleConfidence, 2),
            ProjectCount = model.Projects.Length,
            ProjectNames = [.. model.Projects.OrderBy(p => p.Name).Select(p => p.Name)],
            TypeCount = model.Types.Count,
            Signals = [.. model.Architecture.All
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new KernelSignal(kvp.Key, kvp.Value.Confidence, kvp.Value.Detected))],
        };

        // DC6 — say what was analysed and what was not. Emitted whenever a solution was resolved, so a
        // consumer can distinguish "one system, nothing hidden" from "no note was computed".
        if (model.ScopeNote is { } note)
        {
            output.Scope = new ScopeDto
            {
                Analyzed = note.AnalyzedRelPath,
                AnalyzedName = note.AnalyzedName,
                AnalyzedPath = note.AnalyzedPath,
                SolutionsOnDisk = note.TotalOnDisk,
                Others = [.. note.OtherPaths],
                Requested = note.WasRequested,
                Partial = note.IsPartial,
                Note = note.Text,
            };
        }

        if (snapshot is not null)
        {
            output.EntryCount = snapshot.Entries.IsDefaultOrEmpty ? null : snapshot.Entries.Length;
            output.TopologyProjectCount = snapshot.Map?.Topology.IsDefaultOrEmpty == false ? snapshot.Map.Topology.Length : null;
            output.GraphNodeCount = snapshot.Graph?.NodeCount;
            output.GraphEdgeCount = snapshot.Graph?.EdgeCount;

            if (snapshot.Graph is { EventWiring.IsDefaultOrEmpty: false } g)
            {
                output.EventWiring = new EventWiringDto
                {
                    Total = g.EventWiring.Length,
                    Integration = g.EventWiring.Count(w => w.IsIntegration),
                    CrossService = g.EventWiring.Count(w => w.IsCrossService),
                    Orphan = g.EventWiring.Count(w => w.IsOrphan),
                    Events = [.. g.EventWiring
                        .OrderByDescending(w => w.IsCrossService)
                        .ThenBy(w => w.EventName, StringComparer.Ordinal)
                        .Select(w => new EventWireDto
                        {
                            Event = w.EventName,
                            Integration = w.IsIntegration,
                            CrossService = w.IsCrossService,
                            Publishers = [.. w.Publishers.Select(p => p.Service ?? p.Title).Distinct(StringComparer.Ordinal)],
                            Consumers = [.. w.Consumers.Select(c => c.Service ?? c.Title).Distinct(StringComparer.Ordinal)],
                        })],
                };
            }

            if (!snapshot.Insights.IsDefaultOrEmpty)
            {
                output.Insights = [.. snapshot.Insights.Select(i => new InsightDto
                {
                    Id = i.Id,
                    Category = i.Category.ToString(),
                    Severity = i.Severity.ToString(),
                    Title = i.Title,
                    Detail = "",
                    Evidence = [.. i.Evidence],
                })];
            }
        }

        var json = JsonSerializer.Serialize(output, JsonOptions);
        var estimatedTokens = Math.Max(1, json.Length / 4);

        return new ValueTask<RenderedContext>(new RenderedContext(
            json, estimatedTokens, [.. model.AppliedCompressions], sw.Elapsed, "devcontext/v1"));
    }
}

internal sealed class KernelJsonOutput
{
    public string Schema { get; set; } = "devcontext/v1";
    public DateTimeOffset GeneratedAt { get; set; }
    public string Archetype { get; set; } = "App";
    public string ArchitectureStyle { get; set; } = "Unknown";
    public float StyleConfidence { get; set; }
    public int ProjectCount { get; set; }
    public List<string> ProjectNames { get; set; } = [];
    public int TypeCount { get; set; }
    public int? EntryCount { get; set; }
    public int? TopologyProjectCount { get; set; }
    public int? GraphNodeCount { get; set; }
    public int? GraphEdgeCount { get; set; }
    public List<KernelSignal> Signals { get; set; } = [];
    public List<InsightDto> Insights { get; set; } = [];
    public EventWiringDto? EventWiring { get; set; }
    public ScopeDto? Scope { get; set; }
}

/// <summary>DC6 — which of the repo's systems this analysis covered.</summary>
internal sealed class ScopeDto
{
    public string Analyzed { get; set; } = "";
    public string AnalyzedName { get; set; } = "";
    public string AnalyzedPath { get; set; } = "";
    public int SolutionsOnDisk { get; set; }
    public List<string> Others { get; set; } = [];
    public bool Requested { get; set; }
    public bool Partial { get; set; }
    public string Note { get; set; } = "";
}

internal sealed class EventWiringDto
{
    public int Total { get; set; }
    public int Integration { get; set; }
    public int CrossService { get; set; }
    public int Orphan { get; set; }
    public List<EventWireDto> Events { get; set; } = [];
}

internal sealed class EventWireDto
{
    public string Event { get; set; } = "";
    public bool Integration { get; set; }
    public bool CrossService { get; set; }
    public List<string> Publishers { get; set; } = [];
    public List<string> Consumers { get; set; } = [];
}

internal sealed class InsightDto
{
    public string Id { get; set; } = "";
    public string Category { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public List<string> Evidence { get; set; } = [];
}

internal sealed class KernelSignal(string Key, float Confidence, bool Detected)
{
    public string Key { get; } = Key;
    public float Confidence { get; } = Confidence;
    public bool Detected { get; } = Detected;
}
