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
    public List<SectionToggle> Sections { get; } =
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
        new() { Key = "__source__",                                                Label = "Source code", Hint = "adds full C# bodies, +2k\u201312k tokens" },
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

    public bool IsTraceMode => _selectedScenarioValue == "deep-dive";

    public string DerivedProfile
    {
        get
        {
            var sourceOn = Sections.FirstOrDefault(s => s.Key == "__source__")?.IsEnabled == true;
            var callGraphOn = Sections.FirstOrDefault(s => s.Key == DevContext.Core.Constants.SectionNames.CallGraph)?.IsEnabled == true;
            if (sourceOn) return "full";
            if (callGraphOn) return "debug";
            return "focused";
        }
    }

    public void CompleteInitialization() => _isInitializing = false;

    // ── Public mutators ──────────────────────────────────────────────────────

    public void SetSectionEnabled(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => s.Key == key);
        if (section is null) return;
        section.IsEnabled = enabled;
    }

    public ImmutableArray<string> GetActiveSections()
        => Sections
            .Where(s => s.IsEnabled && s.Key != "__source__")
            .Select(s => s.Key)
            .ToImmutableArray();

    public void LoadSectionDefaults(List<string>? activeSections)
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
        if (IsTraceMode)
        {
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.ArchitectureOverview, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.Endpoints, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MediatRHandlers, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DataModel, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DiRegistrations, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.BackgroundWorkers, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MiddlewarePipeline, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.IndirectWiring, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.CallGraph, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MessageConsumers, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.RelatedTypes, false);
            SetSectionEnabledSilent("__source__", false);
        }
        else
        {
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.ArchitectureOverview, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.Endpoints, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MediatRHandlers, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DataModel, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.DiRegistrations, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.BackgroundWorkers, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MiddlewarePipeline, true);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.IndirectWiring, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.CallGraph, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.MessageConsumers, false);
            SetSectionEnabledSilent(DevContext.Core.Constants.SectionNames.RelatedTypes, true);
            SetSectionEnabledSilent("__source__", false);
        }
    }

    private void SetSectionEnabledSilent(string key, bool enabled)
    {
        var section = Sections.FirstOrDefault(s => s.Key == key);
        if (section is not null) section.IsEnabled = enabled;
    }

    // ── Token math (called from VM after render) ────────────────────────────

    /// <summary>Callback invoked when any section is toggled. The VM wires this to refresh itself.</summary>
    public Action? OnSectionChanged { get; set; }

    /// <summary>Callback invoked when a specific section's include state changes (key, isIncluded).</summary>
    public Action<string, bool>? OnSectionToggled { get; set; }

    public bool IsBuildingSections { get; private set; }

    public (List<SectionGroupViewModel> Groups, string LlmText, int TotalTokens, int SelectedTokens) BuildSectionDataFromStat(
        ImmutableArray<SectionStat> sections)
    {
        IsBuildingSections = true;
        var sectionVms = new List<SectionViewModel>();

        foreach (var stat in sections)
        {
            var key = Sections.FirstOrDefault(s => s.Label == stat.Name)?.Key ?? stat.Name;
            var section = new SectionViewModel
            {
                Key = key,
                Name = stat.Name,
                FullText = "",
                RawTokens = stat.Tokens,
                CompressedTokens = stat.Tokens,
                Category = CategorizeSection(stat.Name),
            };

            section.PropertyChanged += (_, e) =>
            {
                RecalcTokenTotal();
                if (e.PropertyName == nameof(SectionViewModel.IsIncluded))
                    OnSectionToggled?.Invoke(key, section.IsIncluded);
                OnSectionChanged?.Invoke();
            };

            sectionVms.Add(section);
        }

        var groups = new List<SectionGroupViewModel>();
        var categoryOrder = new[] { "API", "Architecture", "Data", "Analysis", "Debug", "Other" };
        foreach (var cat in categoryOrder)
        {
            var children = sectionVms.Where(s => s.Category == cat).ToList();
            if (children.Count == 0) continue;

            var group = new SectionGroupViewModel
            {
                Name = cat,
                IsExpanded = cat != "Debug",
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

        IsBuildingSections = false;
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
                section.IsIncluded = section.Category != "Debug";
        RecalcTokenTotal();
    }

    private static string CategorizeSection(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("endpoint") || lower.Contains("call graph") || lower.Contains("mediatr") || lower.Contains("handler"))
            return "API";
        if (lower.Contains("architecture") || lower.Contains("project") || lower.Contains("di regist") || lower.Contains("non-obvious") || lower.Contains("middleware") || lower.Contains("signal"))
            return "Architecture";
        if (lower.Contains("data model") || lower.Contains("entity") || lower.Contains("message consumer") || lower.Contains("event flow"))
            return "Data";
        if (lower.Contains("anti-pattern") || lower.Contains("related type") || lower.Contains("entry point"))
            return "Analysis";
        if (lower.Contains("diagnostic") || lower.Contains("pruning") || lower.Contains("source code") || lower.Contains("hotpath"))
            return "Debug";
        return "Other";
    }
}
