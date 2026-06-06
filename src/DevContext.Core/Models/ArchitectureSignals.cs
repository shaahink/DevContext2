using System.Collections.Concurrent;

namespace DevContext.Core.Models;

/// <summary>Thread-safe collection of architecture feature signals that can be sealed after Stage 2.</summary>
public sealed class ArchitectureSignals
{
    private readonly ConcurrentDictionary<string, FeatureSignal> _signals = new();
    private volatile bool _sealed;

    /// <summary>Registers a signal, replacing existing signals only if the new confidence is higher. Throws if signals are sealed.</summary>
    public void Register(FeatureSignal signal)
    {
        if (_sealed) throw new InvalidOperationException(
            "Signals sealed after Stage 2. Register signals only in Generic extractors.");
        _signals.AddOrUpdate(signal.Key, signal,
            (_, existing) => signal.Confidence >= existing.Confidence ? signal : existing);
    }

    /// <summary>Returns true if a signal with the given key exists and is detected.</summary>
    public bool Has(string key) => _signals.TryGetValue(key, out var s) && s.Detected;
    /// <summary>Gets a signal by key, or null if not found.</summary>
    public FeatureSignal? Get(string key) => _signals.GetValueOrDefault(key);
    /// <summary>Gets all registered signals.</summary>
    public IReadOnlyDictionary<string, FeatureSignal> All => _signals;
    internal void Seal() => _sealed = true;

    /// <summary>Well-known signal keys used throughout the discovery pipeline.</summary>
    public static class Keys
    {
        public const string MinimalApis = "minimal-apis";
        public const string Controllers = "controllers";
        public const string MediatR = "mediatr";
        public const string EfCore = "efcore";
        public const string MassTransit = "masstransit";
        public const string Aspire = "aspire";
        public const string FastEndpoints = "fast-endpoints";
        public const string Dapper = "dapper";
        public const string Blazor = "blazor";
        public const string WpfMvvm = "wpf-mvvm";
        public const string SignalR = "signalr";
        public const string Grpc = "grpc";
        public const string Scrutor = "scrutor";
        public const string Refit = "refit";
        public const string FluentValidation = "fluentvalidation";
        public const string Hangfire = "hangfire";
        public const string Serilog = "serilog";
        public const string Polly = "polly";
        public const string AutoMapper = "automapper";
        public const string Swagger = "swagger";
        public const string Identity = "identity";
        public const string NLog = "nlog";
        public const string Quartz = "quartz";
        public const string Redis = "redis";
        public const string HealthChecks = "healthchecks";
        public const string NServiceBus = "nservicebus";
    }
}
