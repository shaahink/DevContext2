namespace DevContext.Server.Sessions;

public sealed record ServerOptions
{
    public string Urls { get; init; } = "http://127.0.0.1:5179";
    public int SessionCapacity { get; init; } = 5;
    public TimeSpan SessionIdleTimeout { get; init; } = TimeSpan.FromMinutes(15);
}
