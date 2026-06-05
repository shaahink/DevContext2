namespace DevContext.Core.Models;

public sealed record ExtractionOptions
{
    public ImmutableArray<string> EntryPaths { get; init; } = [];
    public EntryResolutionMode EntryMode { get; init; } = EntryResolutionMode.Auto;
    public ExtractionProfile Profile { get; init; } = ExtractionProfile.Focused;
    public int MaxOutputTokens { get; init; } = 8_000;
    public bool AllowRoslyn { get; init; } = true;
    public TimeSpan RoslynTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public ImmutableArray<string> IncludeExtractors { get; init; } = [];
    public ImmutableArray<string> ExcludeExtractors { get; init; } = [];
    public ImmutableArray<string> ExcludePatterns { get; init; } =
        [".git", "bin", "obj", ".vs", "node_modules", ".idea"];
    public OutputFormat OutputFormat { get; init; } = OutputFormat.Markdown;
    public bool IncludeProvenance { get; init; } = false;
    public bool IncludeDiagnostics { get; init; } = false;
    public bool DryRun { get; init; } = false;
    public int MaxProjects { get; init; } = 150;
}

public enum ExtractionProfile { Quick, Focused, Debug, Full }
public enum OutputFormat { Markdown, Json }
public enum EntryResolutionMode { Auto, FileOnly, FolderOnly, TypeMethod }
