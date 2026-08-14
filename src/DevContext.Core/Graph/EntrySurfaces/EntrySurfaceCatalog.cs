namespace DevContext.Core.Graph.EntrySurfaces;

using DevContext.Core.Models;

/// <summary>
/// The single source of truth for every entry surface shape.
/// Adding a new shape = one descriptor here + one extractor + one builder + one detection record.
/// </summary>
public static class EntrySurfaceCatalog
{
    public static readonly ImmutableArray<EntrySurfaceDescriptor> All =
    [
        // ── App entry surfaces (signal-gated) ────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.Controllers,
            Kind:       EntryPointKind.HttpEndpoint,
            RenderLabel:"HTTP",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        // ── App entry surfaces (no signal gate — detected generically) ──

        new(SignalKey: "",
            Kind:       EntryPointKind.MessageConsumer,
            RenderLabel:"Bus",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: "",
            Kind:       EntryPointKind.DomainEventHandler,
            RenderLabel:"Domain",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: "",
            Kind:       EntryPointKind.HostedService,
            RenderLabel:"Background",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: "",
            Kind:       EntryPointKind.ScheduledJob,
            RenderLabel:"Scheduled",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: "",
            Kind:       EntryPointKind.PublicApi,
            RenderLabel:"Public API",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: ArchitectureSignals.Keys.MinimalApis,
            Kind:       EntryPointKind.HttpEndpoint,
            RenderLabel:"HTTP",
            Role:       SurfaceRole.AppEntry,
            // T1.5 — the minimal-apis signal is what TRIGGERS endpoint extraction (EndpointExtractor
            // ShouldRun gates on it), so the Web SDK hint stays load-bearing. But an OpenApi package is
            // NOT a minimal-API signal — a controllers app that documents itself with OpenAPI/Swagger
            // referenced Microsoft.AspNetCore.OpenApi and got flagged MinimalApi at confidence 1.0,
            // out-ranking its real controllers. Dropped: OpenAPI is orthogonal to the endpoint style.
            Packages:   [],
            SdkHints:   ["Microsoft.NET.Sdk.Web"],
            SelfNamePatterns: []),

        new(SignalKey: ArchitectureSignals.Keys.FastEndpoints,
            Kind:       EntryPointKind.HttpEndpoint,
            RenderLabel:"HTTP",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["FastEndpoints"],
            SdkHints:   [],
            SelfNamePatterns: ["FastEndpoints"]),

        new(SignalKey: ArchitectureSignals.Keys.Blazor,
            Kind:       EntryPointKind.HttpEndpoint,
            RenderLabel:"HTTP",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Microsoft.AspNetCore.Components"],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: ArchitectureSignals.Keys.Grpc,
            Kind:       EntryPointKind.GrpcService,
            RenderLabel:"gRPC",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Grpc.AspNetCore"],
            SdkHints:   [],
            SelfNamePatterns: ["Grpc"]),

        new(SignalKey: ArchitectureSignals.Keys.SignalR,
            Kind:       EntryPointKind.SignalRHub,
            RenderLabel:"SignalR",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Microsoft.AspNetCore.SignalR"],
            SdkHints:   [],
            SelfNamePatterns: ["Microsoft.AspNetCore.SignalR"]),

        new(SignalKey: ArchitectureSignals.Keys.Functions,
            Kind:       EntryPointKind.FunctionEntry,
            RenderLabel:"Functions",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Microsoft.Azure.Functions.Worker"],
            SdkHints:   ["Microsoft.NET.Sdk.Functions"],
            SelfNamePatterns: ["Azure.Functions"]),

        new(SignalKey: ArchitectureSignals.Keys.DesktopUi,
            Kind:       EntryPointKind.UiEntry,
            RenderLabel:"UI",
            Role:       SurfaceRole.AppEntry,
            // D1.2 — "Avalonia" is a family key (prefix matching covers Avalonia.Desktop,
            // Avalonia.Themes.*, Avalonia.ReactiveUI). Avalonia was documented in DesktopEntryExtractor
            // and undetectable: its base types (Window/UserControl) were already handled, but no
            // package or SDK in this catalog fired desktop-ui, so a cross-platform Avalonia head
            // (OutputType Exe, not WinExe) reached nothing. Measured by the avalonia-exe shape case.
            Packages:   ["Microsoft.WindowsAppSDK", "CommunityToolkit.WinUI", "CommunityToolkit.Mvvm", "Avalonia"],
            SdkHints:   ["Microsoft.NET.Sdk.WindowsDesktop"],
            SelfNamePatterns: []),

        // B2 (Prism D1.2b): MAUI. The signal fires from <UseMaui>true</UseMaui> or the mobile TFM
        // triple (DependencyExtractor probes the csproj — UseMaui is SDK-provided on .NET 7+, so
        // package matching alone left podcasts' two MAUI apps invisible). Pages/Shell render as
        // UiEntries via DesktopEntryExtractor.
        new(SignalKey: ArchitectureSignals.Keys.Maui,
            Kind:       EntryPointKind.UiEntry,
            RenderLabel:"UI",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Microsoft.Maui.Controls", "Microsoft.Maui"],
            SdkHints:   [],
            SelfNamePatterns: ["Microsoft.Maui"]),

        // ── Razor Pages — detected via PageModel base types, not packages ──

        new(SignalKey: ArchitectureSignals.Keys.RazorPages,
            Kind:       EntryPointKind.HttpEndpoint,
            RenderLabel:"HTTP",
            Role:       SurfaceRole.AppEntry,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: []),

        // ── E4: Messaging breadth ────────────────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.NServiceBus,
            Kind:       EntryPointKind.MessageConsumer,
            RenderLabel:"Bus",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["NServiceBus", "NServiceBus.Extensions.Hosting"],
            SdkHints:   [],
            SelfNamePatterns: ["NServiceBus"]),

        new(SignalKey: ArchitectureSignals.Keys.AzureServiceBus,
            Kind:       EntryPointKind.MessageConsumer,
            RenderLabel:"Bus",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Azure.Messaging.ServiceBus"],
            SdkHints:   [],
            SelfNamePatterns: ["Azure.Messaging.ServiceBus"]),

        // A4 (Prism D1.1c): nuget id "WolverineFx" != project names "Wolverine.*" — without the
        // self-name pattern the framework's own repo read as a consumer App of itself.
        new(SignalKey: ArchitectureSignals.Keys.Wolverine,
            Kind:       EntryPointKind.MessageConsumer,
            RenderLabel:"Bus",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["WolverineFx"],
            SdkHints:   [],
            SelfNamePatterns: ["Wolverine"]),

        // ── E5: AWS Lambda ───────────────────────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.AwsLambda,
            Kind:       EntryPointKind.FunctionEntry,
            RenderLabel:"Functions",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Amazon.Lambda.Core", "Amazon.Lambda.Annotations"],
            SdkHints:   [],
            SelfNamePatterns: ["Amazon.Lambda"]),

        // ── E6: GraphQL resolvers ────────────────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.GraphQL,
            Kind:       EntryPointKind.GraphQlField,
            RenderLabel:"GraphQL",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["HotChocolate.AspNetCore", "HotChocolate"],
            SdkHints:   [],
            SelfNamePatterns: ["HotChocolate", "GreenDonut"]),

        // ── E7: CLI commands ─────────────────────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.CliCommands,
            Kind:       EntryPointKind.CliCommand,
            RenderLabel:"CLI",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Spectre.Console.Cli", "System.CommandLine"],
            SdkHints:   [],
            SelfNamePatterns: []),

        // ── Framework-library signals (self-source means the repo IS the framework) ──

        new(SignalKey: ArchitectureSignals.Keys.MassTransit,
            Kind:       EntryPointKind.MessageConsumer,
            RenderLabel:"Bus",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["MassTransit"],
            SdkHints:   [],
            SelfNamePatterns: ["MassTransit"]),

        // D1.2 — the packages are what makes this descriptor reachable AT ALL. With `Packages: []` the
        // only mechanism left was SelfNamePatterns, which is a SELF-SOURCE map keyed on the analysed
        // repo's own project names ("this repo IS Orleans") — so a consumer app referencing Orleans
        // fired no signal, OrleansGrainExtractor never ran, and GrainMethod entries could not exist
        // outside Orleans' own repo (which self-sources to Library). Measured by
        // CatalogReachabilityTests. One FAMILY KEY, the same idiom "AspNetCore.HealthChecks" already
        // uses here: TryMatchSignal falls back to prefix matching, so this covers .Server (silo host),
        // .Client, .Sdk (grain + interface classlibs), .Core and every .Persistence.*/.Clustering.*
        // provider. Listing the four leaf ids instead would miss a provider-only project — measured.
        // The self-source guard still wins on Orleans' own repo: a self-sourced key cannot be
        // re-registered from a PackageReference (DependencyExtractor.cs:125, 195).
        new(SignalKey: ArchitectureSignals.Keys.Orleans,
            Kind:       EntryPointKind.GrainMethod,
            RenderLabel:"Orleans",
            Role:       SurfaceRole.AppEntry,
            Packages:   ["Microsoft.Orleans"],
            SdkHints:   [],
            SelfNamePatterns: ["Orleans", "Microsoft.Orleans"]),

        new(SignalKey: ArchitectureSignals.Keys.Quartz,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Quartz"],
            SdkHints:   [],
            SelfNamePatterns: ["Quartz"]),

        new(SignalKey: ArchitectureSignals.Keys.Hangfire,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Hangfire"],
            SdkHints:   [],
            SelfNamePatterns: ["Hangfire"]),

        new(SignalKey: ArchitectureSignals.Keys.Testing,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   [],
            SdkHints:   [],
            SelfNamePatterns: ["xunit", "nunit"]),

        new(SignalKey: ArchitectureSignals.Keys.EfCore,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   [
                "Microsoft.EntityFrameworkCore",
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Microsoft.EntityFrameworkCore.Sqlite",
                "Microsoft.EntityFrameworkCore.InMemory",
                "Npgsql.EntityFrameworkCore.PostgreSQL",
                "Pomelo.EntityFrameworkCore.MySql",
            ],
            SdkHints:   [],
            SelfNamePatterns: ["Microsoft.EntityFrameworkCore"]),

        new(SignalKey: ArchitectureSignals.Keys.Dapper,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Dapper"],
            SdkHints:   [],
            SelfNamePatterns: ["Dapper"]),

        new(SignalKey: ArchitectureSignals.Keys.FluentValidation,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["FluentValidation"],
            SdkHints:   [],
            SelfNamePatterns: ["FluentValidation"]),

        new(SignalKey: ArchitectureSignals.Keys.AutoMapper,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["AutoMapper"],
            SdkHints:   [],
            SelfNamePatterns: ["AutoMapper"]),

        new(SignalKey: ArchitectureSignals.Keys.MediatR,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["MediatR", "Mediator"],
            SdkHints:   [],
            SelfNamePatterns: ["MediatR"]),

        new(SignalKey: ArchitectureSignals.Keys.Serilog,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Serilog"],
            SdkHints:   [],
            SelfNamePatterns: ["Serilog"]),

        new(SignalKey: ArchitectureSignals.Keys.Polly,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Polly"],
            SdkHints:   [],
            SelfNamePatterns: ["Polly"]),

        new(SignalKey: ArchitectureSignals.Keys.Swagger,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Swashbuckle.AspNetCore"],
            SdkHints:   [],
            SelfNamePatterns: ["Swashbuckle"]),

        new(SignalKey: ArchitectureSignals.Keys.Identity,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Microsoft.AspNetCore.Identity"],
            SdkHints:   [],
            SelfNamePatterns: []),

        new(SignalKey: ArchitectureSignals.Keys.NLog,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["NLog"],
            SdkHints:   [],
            SelfNamePatterns: ["NLog"]),

        new(SignalKey: ArchitectureSignals.Keys.Redis,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["StackExchange.Redis"],
            SdkHints:   [],
            SelfNamePatterns: ["StackExchange.Redis"]),

        new(SignalKey: ArchitectureSignals.Keys.HealthChecks,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["AspNetCore.HealthChecks"],
            SdkHints:   [],
            SelfNamePatterns: ["HealthChecks"]),

        new(SignalKey: ArchitectureSignals.Keys.Aspire,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Aspire.Hosting"],
            SdkHints:   ["Aspire.AppHost.Sdk"],
            SelfNamePatterns: ["Aspire.Hosting"]),

        new(SignalKey: ArchitectureSignals.Keys.Scrutor,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Scrutor"],
            SdkHints:   [],
            SelfNamePatterns: ["Scrutor"]),

        new(SignalKey: ArchitectureSignals.Keys.Refit,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.FrameworkLibrary,
            Packages:   ["Refit"],
            SdkHints:   [],
            SelfNamePatterns: ["Refit"]),

        // ── Gateway ─────────────────────────────────────────────────

        new(SignalKey: ArchitectureSignals.Keys.Gateway,
            Kind:       null,
            RenderLabel:"",
            Role:       SurfaceRole.Gateway,
            Packages:   ["Ocelot", "Microsoft.ReverseProxy"],
            SdkHints:   [],
            SelfNamePatterns: ["Yarp", "ReverseProxy"]),
    ];
}
