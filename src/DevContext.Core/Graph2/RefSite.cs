namespace DevContext.Core.Graph2;

public sealed record RefSite
{
    public required string File { get; init; }
    public required int Line { get; init; }
    public required string Project { get; init; }
}
