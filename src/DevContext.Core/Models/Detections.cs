using DevContext.Core.Extractors.Specific;

namespace DevContext.Core.Models;

/// <summary>Base record for all detection types extracted from the codebase. Its JSON polymorphism is
/// declared once, reflectively, in <see cref="DetectionPolymorphism"/> — adding a detection record needs
/// no attribute here (Batch D; there used to be a hand-maintained list that a second scheme overrode).</summary>
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

/// <summary>
/// Marks a detection whose <see cref="Detection.SourceFile"/> declares an application entry surface
/// (a <c>SurfaceRole.AppEntry</c> shape in <see cref="Graph.EntrySurfaces.EntrySurfaceCatalog"/>). Its
/// file seeds Map-mode call-graph binding (<c>CallGraphBinder.EntrySeedFiles</c>) so the entry's
/// handler method gets Calls edges — the basis for entry→target resolution and a non-shallow precomputed
/// Flow. Before T1.1 the seed set was a hardcoded four (endpoints, MediatR, workers, hubs); gRPC/
/// Functions/Orleans/GraphQL/consumer/CLI entries got no call edges in Map mode and traced as shallow as
/// hubs once did. Implementing this on a new AppEntry detection feeds it into the seed set automatically —
/// no <c>CallGraphBinder</c> edit. (Desktop UI stays out until its entry taxonomy is cleaned up in T1.7.)
/// </summary>
public interface IEntrySurfaceDetection;

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
    string? GroupPrefix = null,
    int HandlerLine = 0,
    string? HandlerBody = null
) : Detection, IEntrySurfaceDetection;

/// <summary>
/// App-wide fallback/default authorization policy signal (e.g.
/// <c>AddAuthorization(o =&gt; o.FallbackPolicy = ...RequireAuthenticatedUser())</c>). When present, an
/// endpoint with no per-endpoint or per-group auth metadata is protected by default, not anonymous.
/// </summary>
public sealed record GlobalAuthPolicyDetection(bool HasFallbackPolicy) : Detection;

/// <summary>Detection for a MediatR handler implementation.</summary>
public sealed record MediatRHandlerDetection(
    string RequestType,
    string ResponseType,
    string HandlerType,
    MediatRKind Kind
) : Detection, IEntrySurfaceDetection;

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
) : Detection, IEntrySurfaceDetection;

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
) : Detection, IEntrySurfaceDetection;

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

/// <summary>Kind of desktop UI entry point.</summary>
public enum DesktopEntryKind { Window, Page, UserControl, AppStartup, RelayCommand }

/// <summary>Detection for a desktop UI entry point (Window, Page, UserControl, App.OnLaunched, [RelayCommand]).
/// Batch C (DC4): this is an <see cref="IEntrySurfaceDetection"/> like every other entry surface — it was
/// the one that wasn't, so desktop and MAUI entries never seeded the call-graph closure and a whole
/// archetype had no call spine. eShop's [RelayCommand] entries could only ever resolve to the bare type
/// seam their view-model touched; there were no member-level call edges to resolve through.</summary>
public sealed record DesktopEntryDetection(
    string TypeName,
    DesktopEntryKind Kind,
    string? DeclaringFile = null
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for a gRPC service implementation (class extending XxxBase).</summary>
public sealed record GrpcServiceDetection(
    string ServiceName,
    string ImplementationType,
    ImmutableArray<string> Methods
) : Detection, IEntrySurfaceDetection;

/// <summary>Batch B (DC3) — detection for a transport CLIENT REGISTRATION:
/// <c>AddGrpcClient&lt;T&gt;</c>, typed <c>AddHttpClient&lt;T&gt;</c> or <c>AddRefitClient&lt;T&gt;</c>,
/// carrying the address the client was configured with. The registration site is the only place the
/// TARGET service is named — the injection site sees just a client type, often behind a using-alias
/// (eShop's <c>GrpcBasketClient</c>). <see cref="Address"/> is the raw configured value
/// (e.g. <c>http://basket-api</c>, <c>https+http://catalog-api</c>) or null when none was literal.</summary>
public sealed record TransportClientDetection(
    string Transport,
    string ClientType,
    string? Address
) : Detection;

/// <summary>Transport kinds carried by <see cref="TransportClientDetection.Transport"/>.</summary>
public static class TransportKinds
{
    public const string Grpc = "grpc";
    public const string Http = "http";
    public const string Refit = "refit";
}

/// <summary>Detection for a gRPC generated client type usage (injected XxxClient type).</summary>
public sealed record GrpcClientDetection(
    string ClientType,
    string ServiceName,
    string OwningClass,
    ImmutableArray<string> Methods
) : Detection;

/// <summary>Detection for a SignalR hub class (extends Hub or Hub&lt;T&gt;).</summary>
public sealed record SignalRHubDetection(
    string HubType,
    ImmutableArray<string> HubMethods
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for an Azure Functions entry point (method with [Function] + trigger attributes).</summary>
public sealed record FunctionEntryDetection(
    string ClassName,
    string MethodName,
    ImmutableArray<string> Triggers
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for an Orleans grain class (implements IGrainWithXKey or inherits Grain).</summary>
public sealed record GrainDetection(
    string GrainType,
    string InterfaceType,
    ImmutableArray<string> Methods
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for a GraphQL resolver field (query/mutation/subscription type).</summary>
public sealed record GraphQlFieldDetection(
    string TypeName,
    string FieldName,
    string OperationType
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for a CLI command handler (Spectre.Console.Cli / System.CommandLine).</summary>
/// <param name="CommandName">Batch B — the verb this command is invoked as, when the tool declares
/// one via a <c>[Command("output")]</c>-shaped attribute. Null for base-class-detected commands
/// (the framework assembles the verb) and for the plain-Main fallback.</param>
public sealed record CliCommandDetection(
    string CommandType,
    string SettingsType,
    string ExecuteMethod,
    string? CommandName = null
) : Detection, IEntrySurfaceDetection;

/// <summary>Detection for a Refit HTTP client interface (attributed with [Get]/[Post]/etc.).</summary>
public sealed record RefitRouteDetection(
    string InterfaceType,
    string MethodName,
    string HttpMethod,
    string RouteTemplate
) : Detection;
