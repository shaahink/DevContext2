using System.Collections.Concurrent;

namespace DevContext.Core.Models;

public sealed class ArchitectureSignals
{
    private readonly ConcurrentDictionary<string, FeatureSignal> _signals = new();
    private volatile bool _sealed;

    public void Register(FeatureSignal signal)
    {
        if (_sealed) throw new InvalidOperationException(
            "Signals sealed after Stage 2. Register signals only in Generic extractors.");
        _signals.AddOrUpdate(signal.Key, signal,
            (_, existing) => signal.Confidence >= existing.Confidence ? signal : existing);
    }

    public bool Has(string key) => _signals.TryGetValue(key, out var s) && s.Detected;
    public FeatureSignal? Get(string key) => _signals.GetValueOrDefault(key);
    public IReadOnlyDictionary<string, FeatureSignal> All => _signals;
    internal void Seal() => _sealed = true;

    public static class Keys
    {
        public const string MinimalApis      = "minimal-apis";
        public const string Controllers      = "controllers";
        public const string MediatR          = "mediatr";
        public const string EfCore           = "efcore";
        public const string MassTransit      = "masstransit";
        public const string Aspire           = "aspire";
        public const string FastEndpoints    = "fast-endpoints";
        public const string Dapper           = "dapper";
        public const string Blazor           = "blazor";
        public const string WpfMvvm          = "wpf-mvvm";
        public const string SignalR          = "signalr";
        public const string Grpc             = "grpc";
        public const string Scrutor          = "scrutor";
        public const string Refit            = "refit";
        public const string FluentValidation = "fluentvalidation";
        public const string Hangfire         = "hangfire";
    }
}
