using System.Text.Json.Serialization;

using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Models;

/// <summary>Base record for all detection types extracted from the codebase.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EndpointDetection), "EndpointDetection")]
[JsonDerivedType(typeof(MediatRHandlerDetection), "MediatRHandlerDetection")]
[JsonDerivedType(typeof(EfEntityDetection), "EfEntityDetection")]
[JsonDerivedType(typeof(BackgroundWorkerDetection), "BackgroundWorkerDetection")]
[JsonDerivedType(typeof(MiddlewareDetection), "MiddlewareDetection")]
[JsonDerivedType(typeof(IndirectWiringDetection), "IndirectWiringDetection")]
[JsonDerivedType(typeof(MessageConsumerDetection), "MessageConsumerDetection")]
[JsonDerivedType(typeof(DiRegistrationDetection), "DiRegistrationDetection")]
[JsonDerivedType(typeof(AspireResourceDetection), "AspireResourceDetection")]
[JsonDerivedType(typeof(AspireRelationshipDetection), "AspireRelationshipDetection")]
[JsonDerivedType(typeof(AntiPatternDetection), "AntiPatternDetection")]
[JsonDerivedType(typeof(EventFlowDetection), "EventFlowDetection")]
public abstract record Detection
{
    /// <summary>Name of the extractor that produced this detection.</summary>
    public required string ExtractorName { get; init; }
    /// <summary>Source file where the detection was found.</summary>
    public required string SourceFile { get; init; }
    /// <summary>Line number in the source file.</summary>
    public required int LineNumber { get; init; }
    /// <summary>Confidence level of this detection (0.0 to 1.0).</summary>
    public float Confidence { get; init; } = 1.0f;
}

/// <summary>Categorizes a MediatR handler as a Command, Query, or Notification handler.</summary>
public enum MediatRKind { Command, Query, Notification }
/// <summary>Describes the kind of background worker.</summary>
public enum BackgroundWorkerKind { HostedService, BackgroundService, TimedJob }
/// <summary>Describes how middleware is registered.</summary>
public enum MiddlewareKind { UseX, MapX, CustomClass }
/// <summary>Describes the kind of indirect wiring detected.</summary>
public enum IndirectWiringKind { ReflectionActivation, DynamicProxy, ManualServiceLocator }

/// <summary>Detection for an HTTP endpoint (controller action or minimal API).</summary>
public sealed record EndpointDetection(
    string HttpMethod,
    string RouteTemplate,
    string HandlerType,
    string HandlerMethod,
    ImmutableArray<string> AuthAttributes,
    ImmutableArray<string> ParameterTypes,
    string? GroupPrefix = null
) : Detection;

/// <summary>Detection for a MediatR handler implementation.</summary>
public sealed record MediatRHandlerDetection(
    string RequestType,
    string ResponseType,
    string HandlerType,
    MediatRKind Kind
) : Detection;

/// <summary>Detection for an EF Core entity and its DbSet registration.</summary>
public sealed record EfEntityDetection(
    string EntityType,
    string DbContextType,
    bool IsAggregate,
    ImmutableArray<string> KeyProperties
) : Detection;

/// <summary>Detection for a background worker (hosted service, etc.).</summary>
public sealed record BackgroundWorkerDetection(
    string ServiceType,
    string ImplementationType,
    BackgroundWorkerKind Kind
) : Detection;

/// <summary>Detection for registered middleware in the pipeline.</summary>
public sealed record MiddlewareDetection(
    string MiddlewareType,
    int PipelineOrder,
    MiddlewareKind Kind
) : Detection;

/// <summary>Detection for indirect wiring patterns (reflection, service locator, dynamic proxy).</summary>
public sealed record IndirectWiringDetection(
    IndirectWiringKind Kind,
    string CallerType,
    string CallerMethod,
    string? TargetType
) : Detection;

/// <summary>Detection for a message bus consumer registration.</summary>
public sealed record MessageConsumerDetection(
    string MessageType,
    string ConsumerType,
    string BusKind
) : Detection;

/// <summary>Classification of how a DI registration binds service to implementation.</summary>
public enum DiRegistrationShape
{
    /// <summary>AddScoped&lt;IFoo, Foo&gt;() — explicit interface-to-implementation mapping.</summary>
    DirectBinding,
    /// <summary>AddSingleton&lt;Foo&gt;() — self-registration, no interface.</summary>
    SelfRegistration,
    /// <summary>sp => sp.GetRequiredService&lt;Foo&gt;() — pure alias, no factory logic.</summary>
    ForwardingAlias,
    /// <summary>sp => new Foo(...) or sp => { ... } — has real factory logic.</summary>
    InlineFactory,
}

public sealed record DiRegistrationDetection(
    string ServiceType,
    string ImplementationType,
    string Lifetime,
    ImmutableArray<string> ExtensionsUsed,
    DiRegistrationShape Shape = DiRegistrationShape.DirectBinding,
    string? FactorySummary = null
) : Detection;

/// <summary>Detection for an anti-pattern found in the codebase.</summary>
public sealed record AntiPatternDetection(
    string Pattern,
    string Description,
    string Severity,
    string TargetType
) : Detection;
