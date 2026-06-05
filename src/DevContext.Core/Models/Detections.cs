namespace DevContext.Core.Models;

public abstract record Detection
{
    public required string ExtractorName { get; init; }
    public required string SourceFile { get; init; }
    public required int LineNumber { get; init; }
    public float Confidence { get; init; } = 1.0f;
}

public enum MediatRKind { Command, Query, Notification }
public enum BackgroundWorkerKind { HostedService, BackgroundService, TimedJob }
public enum MiddlewareKind { UseX, MapX, CustomClass }
public enum IndirectWiringKind { ReflectionActivation, DynamicProxy, ManualServiceLocator }

public sealed record EndpointDetection(
    string HttpMethod,
    string RouteTemplate,
    string HandlerType,
    string HandlerMethod,
    ImmutableArray<string> AuthAttributes,
    ImmutableArray<string> ParameterTypes
) : Detection;

public sealed record MediatRHandlerDetection(
    string RequestType,
    string ResponseType,
    string HandlerType,
    MediatRKind Kind
) : Detection;

public sealed record EfEntityDetection(
    string EntityType,
    string DbContextType,
    bool IsAggregate,
    ImmutableArray<string> KeyProperties
) : Detection;

public sealed record BackgroundWorkerDetection(
    string ServiceType,
    string ImplementationType,
    BackgroundWorkerKind Kind
) : Detection;

public sealed record MiddlewareDetection(
    string MiddlewareType,
    int PipelineOrder,
    MiddlewareKind Kind
) : Detection;

public sealed record IndirectWiringDetection(
    IndirectWiringKind Kind,
    string CallerType,
    string CallerMethod,
    string? TargetType
) : Detection;

public sealed record MessageConsumerDetection(
    string MessageType,
    string ConsumerType,
    string BusKind
) : Detection;
