using System.Collections.Immutable;
using DevContext.Core.Contracts;

namespace DevContext.Desktop.ViewModels;

public sealed class SectionToggle
{
    public string Key { get; init; } = "";
    public string Label { get; init; } = "";
    public string? Hint { get; init; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>Pure model for section toggling, profile derivation, and token math. Has no WPF/Blazor dependencies.</summary>
public sealed class SectionSelectionModel
{
    private bool _isInitializing = true;

    // ── Section toggle list ──────────────────────────────────────────────────
    public IList<SectionToggle> Sections { get; } =
    [
        new() { Key = DevContext.Core.Constants.SectionNames.ArchitectureOverview, Label = "Architecture overview" },
        new() { Key = DevContext.Core.Constants.SectionNames.Endpoints,            Label = "Endpoints" },
        new() { Key = DevContext.Core.Constants.SectionNames.MediatRHandlers,      Label = "MediatR Handlers" },
        new() { Key = DevContext.Core.Constants.SectionNames.DataModel,            Label = "Data model" },
        new() { Key = DevContext.Core.Constants.SectionNames.DiRegistrations,      Label = "DI registrations" },
        new() { Key = DevContext.Core.Constants.SectionNames.BackgroundWorkers,    Label = "Background workers" },
        new() { Key = DevContext.Core.Constants.SectionNames.MiddlewarePipeline,   Label = "Middleware pipeline" },
        new() { Key = DevContext.Core.Constants.SectionNames.IndirectWiring,       Label = "Indirect wiring" },
        new() { Key = DevContext.Core.Constants.SectionNames.CallGraph,            Label = "Call graph", Hint = "+call graph, needs Roslyn" },
        new() { Key = DevContext.Core.Constants.SectionNames.MessageConsumers,     Label = "Message consumers" },
        new() { Key = DevContext.Core.Constants.SectionNames.RelatedTypes,         Label = "Related types" },
        new() { Key = SourceSectionKey,                                                Label = "Source code", Hint = "adds full C# bodies, +2k\u201312k tokens" },
    ];

    // ── Section groups (computed from rendered sections) ──────────────────────
    public ImmutableArray<SectionGroupViewModel> SectionGroups { get; private set; }
        = ImmutableArray<SectionGroupViewModel>.Empty;

    private int _selectedTokenTotal;
    public int SelectedTokenTotal => _selectedTokenTotal;

    private int _totalTokens;
    public int TotalTokens => _totalTokens;

    private int _budgetTokens = 8000;
    public int BudgetTokens
    {
        get => _budgetTokens;
        set => _budgetTokens = value;
    }

    public float BudgetUtilisation => _budgetTokens > 0
        ? (float)_totalTokens / _budgetTokens
        : 0;

    // ── Profile / scenario ───────────────────────────────────────────────────
    private string _selectedScenarioValue = "overview";
    public string SelectedScenarioValue
    {
        get => _selectedScenarioValue;
        set
        {
            _selectedScenarioValue = value;
            if (!_isInitializing) ApplyScenarioSectionDefaults();
        }
    }

    public bool IsTraceMode => string.Equals(_selectedScenarioValue, "deep-dive", StringComparison.Ordinal);

    public string DerivedProfile
    {
        get
        {
            var sourceOn = Sections.FirstOrDefault(s => string.Equals(s.Key, SourceSectionKey, StringComparison.Ordinal))?.IsEnabled == true;
            var callGraphOn = Sections.FirstOrDefault(s => string.Equals(s.Key, DevContext.Core.Constants.SectionNames.CallGraph, StringComparison.Ordinal))?.IsEnabled == true;
            if (sourceOn) return "full";
            if (callGraphOn) return "debug";
            return "focused";
        }
    }

    public void CompleteInitialization() => _isInitializing = false;

    // ── Public mutators ──────────────────────────────────────────────────────

    public void SetSectionEnabled(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.Ordinal));
        if (section is null) return;
        section.IsEnabled = enabled;
    }

    public ImmutableArray<string> GetActiveSections()
        => Sections
            .Where(s => s.IsEnabled && !string.Equals(s.Key, SourceSectionKey, StringComparison.Ordinal))
            .Select(s => s.Key)
            .ToImmutableArray();

    public void LoadSectionDefaults(IList<string>? activeSections)
    {
        if (activeSections is { Count: > 0 })
        {
            foreach (var section in Sections)
                section.IsEnabled = activeSections.Contains(section.Key);
        }
        else
        {
            ApplyScenarioSectionDefaults();
        }
    }

    public void ApplyScenarioSectionDefaults()
    {
        // Section defaults keyed by scenario mode: (Section, TraceEnabled, OverviewEnabled)
        foreach (var (key, traceEnabled, overviewEnabled) in SectionDefaultsMap)
            SetSectionEnabledSilent(key, IsTraceMode ? traceEnabled : overviewEnabled);
    }

    private static readonly (string Key, bool Trace, bool Overview)[] SectionDefaultsMap =
    [
        (DevContext.Core.Constants.SectionNames.ArchitectureOverview, false, true),
        (DevContext.Core.Constants.SectionNames.Endpoints, true, true),
        (DevContext.Core.Constants.SectionNames.MediatRHandlers, true, true),
        (DevContext.Core.Constants.SectionNames.DataModel, false, true),
        (DevContext.Core.Constants.SectionNames.DiRegistrations, true, true),
        (DevContext.Core.Constants.SectionNames.BackgroundWorkers, true, false),
        (DevContext.Core.Constants.SectionNames.MiddlewarePipeline, true, true),
        (DevContext.Core.Constants.SectionNames.IndirectWiring, false, false),
        (DevContext.Core.Constants.SectionNames.CallGraph, true, false),
        (DevContext.Core.Constants.SectionNames.MessageConsumers, false, false),
        (DevContext.Core.Constants.SectionNames.RelatedTypes, false, true),
        ("__source__", false, false),
    ];

    private const string SourceSectionKey = "__source__";

    private void SetSectionEnabledSilent(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => string.Equals(s.Key, key, StringComparison.Ordinal));
        if (section is not null) section.IsEnabled = enabled;
    }

    // ── Token math (called from VM after render) ────────────────────────────

    /// <summary>Callback invoked when any section is toggled. The VM wires this to refresh itself.</summary>
    public Action? OnSectionChanged { get; set; }

    public (List<SectionGroupViewModel> Groups, string LlmText, int TotalTokens, int SelectedTokens) BuildSectionDataFromStat(
        ImmutableArray<SectionStat> sections)
    {
        var sectionVms = new List<SectionViewModel>();

        foreach (var stat in sections)
        {
            var section = new SectionViewModel
            {
                Name = stat.Name,
                FullText = "",
                RawTokens = stat.Tokens,
                CompressedTokens = stat.Tokens,
                Category = CategorizeSection(stat.Name),
            };

            section.PropertyChanged += (_, _) =>
            {
                RecalcTokenTotal();
                OnSectionChanged?.Invoke();
            };

            sectionVms.Add(section);
        }

        var groups = new List<SectionGroupViewModel>();
        var categoryOrder = new[] { "API", "Architecture", "Data", "Analysis", "Debug", "Other" };
        foreach (var cat in categoryOrder)
        {
            var children = sectionVms.Where(s => string.Equals(s.Category, cat, StringComparison.Ordinal)).ToList();
            if (children.Count == 0) continue;

            var group = new SectionGroupViewModel
            {
                Name = cat,
                IsExpanded = !string.Equals(cat, "Debug", StringComparison.Ordinal),
            };
            group.Children.AddRange(children);
            group.PropertyChanged += (_, _) => RecalcTokenTotal();
            groups.Add(group);
        }

        SectionGroups = groups.ToImmutableArray();

        var totalTokens = sectionVms.Sum(s => s.CompressedTokens);
        var selectedTokens = sectionVms.Where(s => s.IsIncluded).Sum(s => s.CompressedTokens);
        var llmText = string.Join(Environment.NewLine,
            sectionVms.Where(s => s.IsIncluded).Select(s => s.FullText));

        _totalTokens = totalTokens;
        _selectedTokenTotal = selectedTokens;

        return (groups, llmText, totalTokens, selectedTokens);
    }

    public void RecalcTokenTotal()
    {
        _selectedTokenTotal = SectionGroups
            .SelectMany(g => g.Children)
            .Where(s => s.IsIncluded)
            .Sum(s => s.CompressedTokens);
    }

    public void ResetToDefaults()
    {
        foreach (var group in SectionGroups)
            foreach (var section in group.Children)
                section.IsIncluded = !string.Equals(section.Category, "Debug", StringComparison.Ordinal);
        RecalcTokenTotal();
    }

    private static string CategorizeSection(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("endpoint", StringComparison.Ordinal) || lower.Contains("call graph", StringComparison.Ordinal) || lower.Contains("mediatr", StringComparison.Ordinal) || lower.Contains("handler", StringComparison.Ordinal))
            return "API";
        if (lower.Contains("architecture", StringComparison.Ordinal) || lower.Contains("project", StringComparison.Ordinal) || lower.Contains("di regist", StringComparison.Ordinal) || lower.Contains("non-obvious", StringComparison.Ordinal) || lower.Contains("middleware", StringComparison.Ordinal) || lower.Contains("signal", StringComparison.Ordinal))
            return "Architecture";
        if (lower.Contains("data model", StringComparison.Ordinal) || lower.Contains("entity", StringComparison.Ordinal) || lower.Contains("message consumer", StringComparison.Ordinal) || lower.Contains("event flow", StringComparison.Ordinal))
            return "Data";
        if (lower.Contains("anti-pattern", StringComparison.Ordinal) || lower.Contains("related type", StringComparison.Ordinal) || lower.Contains("entry point", StringComparison.Ordinal))
            return "Analysis";
        if (lower.Contains("diagnostic", StringComparison.Ordinal) || lower.Contains("pruning", StringComparison.Ordinal) || lower.Contains("source code", StringComparison.Ordinal) || lower.Contains("hotpath", StringComparison.Ordinal))
            return "Debug";
        return "Other";
    }
}
