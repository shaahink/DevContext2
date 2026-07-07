namespace DevContext.Core.Graph2;

public sealed record RefSite
{
    public required string File { get; init; }
    public required int Line { get; init; }
    public required string Project { get; init; }

    public static RefSite FromType(TypeDiscovery t) => new()
    {
        File = t.FilePath,
        Line = t.StartLine ?? 0,
        Project = "",
    };
}
