using System.Text;

using DevContext.Core.Graph;
using DevContext.Core.Graph.EntrySurfaces;

using Xunit;

namespace DevContext.Core.Tests;

/// <summary>
/// D1.1 — the CATALOG-REACHABILITY instrument. The detection twin of the dogfood edge invariant and
/// the contract sweep: a permanently red/green property over the whole <see cref="EntrySurfaceCatalog"/>
/// instead of a hand audit that goes stale.
///
/// The class of defect it pins is <b>the catalog over-declares</b> — a descriptor claims a shape, so
/// nothing looks missing, while no consumer app can ever reach it. Two properties, both MEASURED by
/// running the real pipeline over a generated consumer-app repo, never read off a doc comment:
///
/// <list type="number">
/// <item><b>P1 SIGNAL REACHABLE</b> — for every descriptor with a signal key, a consumer app whose
/// csproj carries only <i>that descriptor's own declared</i> Packages/SdkHints, plus the source shape
/// a consumer would write, fires the signal. The projects are NEUTRALLY NAMED on purpose: a consumer
/// app cannot fire a signal through <c>SelfNamePatterns</c> (that map keys on the analysed repo's own
/// project names and means "this repo IS the framework"), so a descriptor whose only mechanism is a
/// self-name is unreachable — which is exactly the Orleans find.</item>
/// <item><b>P2 KIND PRODUCED</b> — for every descriptor that declares an <see cref="EntryPointKind"/>,
/// that descriptor's own project produces at least one entry of that kind: the full
/// descriptor→signal→extractor→builder→entry path, end to end.</item>
/// </list>
///
/// Known holes live in <c>eval/expectations/catalog-reachability-allow.txt</c> with a measured reason
/// each, and the file is a RATCHET, not a mute: the sweep fails both when something outside the file
/// is unreachable AND when something inside it has become reachable (close the hole, delete the line).
///
/// Fixture rules that keep the measurement honest — change them and you weaken the gate:
/// (a) a project for a descriptor WITH a signal key declares no package or SDK the catalog did not
///     declare for it, so a green can only come from the catalog's own declaration;
/// (b) project names are <c>Consumer{n}</c> — never a framework name;
/// (c) each shape is what a consumer app would NATURALLY write, not the one path known to work
///     (the scheduled-job shape is a timer-driven worker, not the DNTScheduler-only registration).
/// Known limit of a single-repo sweep: signals are repo-wide, so P2's extractor gate can be satisfied
/// by another project's signal. P1 is the per-descriptor half of the pair, and it is not maskable
/// that way because every project declares only its own descriptor's packages.
/// </summary>
[Trait("Category", "Truth")]
public sealed class CatalogReachabilityTests
{
    // ── The consumer-app shapes, one per descriptor ──────────────────────────────────────────────
    // Key = the descriptor's signal key, or "kind:<Kind>" for the six signal-less descriptors.
    // Packages and SDK come from the CATALOG — they are the declaration under test, not part of the
    // recipe. Only the source shape lives here.

    private static readonly Dictionary<string, (string Path, string Content)[]> Shapes = new(StringComparer.Ordinal)
    {
        [ArchitectureSignals.Keys.Controllers] =
        [
            ("Api/OrdersController.cs", """
                using Microsoft.AspNetCore.Mvc;

                namespace Consumer.Api;

                [ApiController]
                [Route("orders")]
                public sealed class OrdersController : ControllerBase
                {
                    [HttpGet("{id}")]
                    public string GetOrder(int id) => id.ToString();
                }
                """),
        ],

        [ArchitectureSignals.Keys.MinimalApis] =
        [
            ("Program.cs", """
                var builder = WebApplication.CreateBuilder(args);
                var app = builder.Build();
                app.MapGet("/ping", () => "pong");
                app.Run();
                """),
        ],

        [ArchitectureSignals.Keys.FastEndpoints] =
        [
            ("Endpoints/GetOrderEndpoint.cs", """
                using FastEndpoints;

                namespace Consumer.Endpoints;

                public sealed class GetOrderRequest { public int Id { get; set; } }

                public sealed class GetOrderEndpoint : Endpoint<GetOrderRequest, string>
                {
                    public override void Configure()
                    {
                        Get("/orders/{id}");
                        AllowAnonymous();
                    }

                    public override Task HandleAsync(GetOrderRequest req, CancellationToken ct)
                        => SendAsync(req.Id.ToString(), cancellation: ct);
                }
                """),
        ],

        [ArchitectureSignals.Keys.Blazor] =
        [
            ("Pages/Counter.razor", """
                @page "/counter"

                <h1>Counter</h1>
                <button @onclick="Increment">Click</button>

                @code {
                    private int _count;
                    private void Increment() => _count++;
                }
                """),
        ],

        [ArchitectureSignals.Keys.Grpc] =
        [
            // Hand-stub of what protoc generates, the shape GrpcServiceExtractor keys on.
            ("Rpc/GreeterGenerated.cs", """
                namespace Consumer.Rpc;

                public static class Greeter
                {
                    public abstract class GreeterBase
                    {
                        public virtual Task<HelloReply> SayHello(HelloRequest request)
                            => Task.FromResult(new HelloReply(string.Empty));
                    }
                }

                public sealed record HelloRequest(string Name);
                public sealed record HelloReply(string Message);
                """),
            ("Rpc/GreeterService.cs", """
                namespace Consumer.Rpc;

                public sealed class GreeterService : Greeter.GreeterBase
                {
                    public override Task<HelloReply> SayHello(HelloRequest request)
                        => Task.FromResult(new HelloReply(request.Name));
                }
                """),
        ],

        [ArchitectureSignals.Keys.SignalR] =
        [
            ("Hubs/ChatHub.cs", """
                using Microsoft.AspNetCore.SignalR;

                namespace Consumer.Hubs;

                public sealed class ChatHub : Hub
                {
                    public Task Broadcast(string message) => Clients.All.SendAsync("received", message);
                }
                """),
        ],

        [ArchitectureSignals.Keys.Functions] =
        [
            ("Functions/OrderIngestFunction.cs", """
                using Microsoft.Azure.Functions.Worker;

                namespace Consumer.Functions;

                public sealed class OrderIngestFunction
                {
                    [Function("IngestOrder")]
                    public Task Run([QueueTrigger("orders")] string payload) => Task.CompletedTask;
                }
                """),
        ],

        [ArchitectureSignals.Keys.DesktopUi] =
        [
            ("MainWindow.xaml.cs", """
                using System.Windows;

                namespace Consumer;

                public partial class MainWindow : Window
                {
                    public MainWindow() => InitializeComponent();

                    private void OnSave(object sender, RoutedEventArgs e) { }
                }
                """),
            ("App.xaml.cs", """
                using System.Windows;

                namespace Consumer;

                public partial class App : Application
                {
                }
                """),
        ],

        [ArchitectureSignals.Keys.Maui] =
        [
            ("MainPage.xaml.cs", """
                using Microsoft.Maui.Controls;

                namespace Consumer;

                public partial class MainPage : ContentPage
                {
                    public MainPage() => InitializeComponent();

                    private void OnCounterClicked(object sender, EventArgs e) { }
                }
                """),
        ],

        [ArchitectureSignals.Keys.RazorPages] =
        [
            ("Pages/Index.cshtml.cs", """
                using Microsoft.AspNetCore.Mvc.RazorPages;

                namespace Consumer.Pages;

                public sealed class IndexModel : PageModel
                {
                    public void OnGet() { }
                }
                """),
            ("Pages/Index.cshtml", """
                @page
                @model Consumer.Pages.IndexModel
                <h1>Index</h1>
                """),
        ],

        [ArchitectureSignals.Keys.NServiceBus] =
        [
            ("Handlers/OrderPlacedHandler.cs", """
                using NServiceBus;

                namespace Consumer.Handlers;

                public sealed class OrderPlaced { public int Id { get; set; } }

                public sealed class OrderPlacedHandler : IHandleMessages<OrderPlaced>
                {
                    public Task Handle(OrderPlaced message, IMessageHandlerContext context)
                        => Task.CompletedTask;
                }
                """),
        ],

        [ArchitectureSignals.Keys.AzureServiceBus] =
        [
            ("Messaging/OrderQueueListener.cs", """
                using Azure.Messaging.ServiceBus;

                namespace Consumer.Messaging;

                public sealed class OrderQueueListener
                {
                    private readonly ServiceBusProcessor _processor;

                    public OrderQueueListener(ServiceBusClient client)
                    {
                        _processor = client.CreateProcessor("orders");
                        _processor.ProcessMessageAsync += OnMessageAsync;
                    }

                    private Task OnMessageAsync(ProcessMessageEventArgs args) => Task.CompletedTask;
                }
                """),
        ],

        [ArchitectureSignals.Keys.Wolverine] =
        [
            ("Handlers/InvoiceApprovedHandler.cs", """
                namespace Consumer.Handlers;

                public sealed class InvoiceApproved { public int Id { get; set; } }

                public sealed class InvoiceApprovedHandler
                {
                    public void Handle(InvoiceApproved message) { }
                }
                """),
        ],

        [ArchitectureSignals.Keys.AwsLambda] =
        [
            ("Function.cs", """
                using Amazon.Lambda.Core;

                namespace Consumer;

                public sealed class Function
                {
                    public string FunctionHandler(string input, ILambdaContext context) => input.ToUpperInvariant();
                }
                """),
        ],

        [ArchitectureSignals.Keys.GraphQL] =
        [
            ("GraphQl/CatalogQuery.cs", """
                using HotChocolate;
                using HotChocolate.Types;

                namespace Consumer.GraphQl;

                [QueryType]
                public sealed class CatalogQuery
                {
                    public string GetProduct(int id) => id.ToString();
                }
                """),
        ],

        [ArchitectureSignals.Keys.CliCommands] =
        [
            ("Commands/BuildCommand.cs", """
                using Spectre.Console.Cli;

                namespace Consumer.Commands;

                public sealed class BuildCommand : Command<BuildCommand.Settings>
                {
                    public sealed class Settings : CommandSettings
                    {
                        [CommandArgument(0, "<project>")]
                        public string Project { get; set; } = "";
                    }

                    public override int Execute(CommandContext context, Settings settings) => 0;
                }
                """),
            ("Program.cs", """
                using Spectre.Console.Cli;

                using Consumer.Commands;

                var app = new CommandApp();
                app.Configure(config => config.AddCommand<BuildCommand>("build"));
                return app.Run(args);
                """),
        ],

        [ArchitectureSignals.Keys.MassTransit] =
        [
            ("Consumers/ShipmentDispatchedConsumer.cs", """
                using MassTransit;

                namespace Consumer.Consumers;

                public sealed class ShipmentDispatched { public int Id { get; set; } }

                public sealed class ShipmentDispatchedConsumer : IConsumer<ShipmentDispatched>
                {
                    public Task Consume(ConsumeContext<ShipmentDispatched> context) => Task.CompletedTask;
                }
                """),
        ],

        [ArchitectureSignals.Keys.Orleans] =
        [
            ("Grains/UserGrain.cs", """
                using Orleans;

                namespace Consumer.Grains;

                public interface IUserGrain : IGrainWithStringKey
                {
                    Task<string> GetDisplayName();
                }

                public sealed class UserGrain : Grain, IUserGrain
                {
                    public Task<string> GetDisplayName() => Task.FromResult("user");
                }
                """),
        ],

        // ── The six signal-less descriptors: the kind is the whole declaration ───────────────────

        ["kind:" + nameof(EntryPointKind.MessageConsumer)] =
        [
            ("Events/StockChangedIntegrationEventHandler.cs", """
                namespace Consumer.Events;

                public sealed class StockChangedIntegrationEvent { public int ProductId { get; set; } }

                public sealed class StockChangedIntegrationEventHandler
                    : IIntegrationEventHandler<StockChangedIntegrationEvent>
                {
                    public Task Handle(StockChangedIntegrationEvent @event) => Task.CompletedTask;
                }
                """),
        ],

        ["kind:" + nameof(EntryPointKind.DomainEventHandler)] =
        [
            ("Domain/BasketCheckedOutDomainEventHandler.cs", """
                using MediatR;

                namespace Consumer.Domain;

                public sealed class BasketCheckedOutDomainEvent : INotification { public int BasketId { get; set; } }

                public sealed class BasketCheckedOutDomainEventHandler
                    : INotificationHandler<BasketCheckedOutDomainEvent>
                {
                    public Task Handle(BasketCheckedOutDomainEvent notification, CancellationToken cancellationToken)
                        => Task.CompletedTask;
                }
                """),
        ],

        ["kind:" + nameof(EntryPointKind.HostedService)] =
        [
            ("Workers/OutboxPublisher.cs", """
                using Microsoft.Extensions.Hosting;

                namespace Consumer.Workers;

                public sealed class OutboxPublisher : BackgroundService
                {
                    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
                }
                """),
            ("Program.cs", """
                using Consumer.Workers;

                var builder = Host.CreateApplicationBuilder(args);
                builder.Services.AddHostedService<OutboxPublisher>();
                builder.Build().Run();
                """),
        ],

        // The shape a consumer app ACTUALLY writes for "run this every night": a timer inside a
        // BackgroundService. Not DNTScheduler's AddScheduledTask<T>, which is the one path the
        // ScheduledJob kind has left.
        ["kind:" + nameof(EntryPointKind.ScheduledJob)] =
        [
            ("Jobs/NightlyReconciliationJob.cs", """
                using Microsoft.Extensions.Hosting;

                namespace Consumer.Jobs;

                public sealed class NightlyReconciliationJob : BackgroundService
                {
                    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                    {
                        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
                        while (await timer.WaitForNextTickAsync(stoppingToken))
                            await ReconcileAsync(stoppingToken);
                    }

                    private Task ReconcileAsync(CancellationToken ct) => Task.CompletedTask;
                }
                """),
            ("Program.cs", """
                using Consumer.Jobs;

                var builder = Host.CreateApplicationBuilder(args);
                builder.Services.AddHostedService<NightlyReconciliationJob>();
                builder.Build().Run();
                """),
        ],

        ["kind:" + nameof(EntryPointKind.PublicApi)] =
        [
            ("Client/PaymentsClient.cs", """
                namespace Consumer.Client;

                /// <summary>The public surface of a library a consumer app calls into.</summary>
                public sealed class PaymentsClient
                {
                    public Task<bool> ChargeAsync(string customerId, decimal amount) => Task.FromResult(true);

                    public Task RefundAsync(string chargeId) => Task.CompletedTask;
                }
                """),
        ],
    };

    /// <summary>Extra csproj properties a shape needs to be the app it claims to be.</summary>
    private static readonly Dictionary<string, (string Name, string Value)[]> Props = new(StringComparer.Ordinal)
    {
        [ArchitectureSignals.Keys.DesktopUi] = [("OutputType", "WinExe"), ("UseWPF", "true")],
        [ArchitectureSignals.Keys.Maui] = [("OutputType", "Exe"), ("UseMaui", "true")],
        [ArchitectureSignals.Keys.CliCommands] = [("OutputType", "Exe")],
        ["kind:" + nameof(EntryPointKind.HostedService)] = [("OutputType", "Exe")],
        ["kind:" + nameof(EntryPointKind.ScheduledJob)] = [("OutputType", "Exe")],
    };

    private static string KeyOf(EntrySurfaceDescriptor d) =>
        d.SignalKey.Length > 0 ? d.SignalKey : "kind:" + d.Kind;

    // ── The sweep ────────────────────────────────────────────────────────────────────────────────

    private sealed record Row(
        string Key, string Project, string SignalKey, EntryPointKind? Kind,
        bool SignalFired, string Via, bool KindProduced, string Produced);

    private sealed record SweepResult(ImmutableArray<Row> Rows, string RepoPath, int EntryCount);

    private static readonly Lazy<Task<SweepResult>> Sweep = new(SweepAsync);

    private static async Task<SweepResult> SweepAsync()
    {
        var repo = Path.Combine(Path.GetTempPath(), "dc2-catalog-coverage");
        if (Directory.Exists(repo)) Directory.Delete(repo, recursive: true);
        Directory.CreateDirectory(repo);

        var projects = new List<(EntrySurfaceDescriptor D, string Name)>();
        var slnx = new StringBuilder().AppendLine("<Solution>").AppendLine("  <Folder Name=\"/src/\">");

        var index = 0;
        foreach (var d in EntrySurfaceCatalog.All)
        {
            index++;
            // Neutral name — a framework name here would fire the signal through SelfNamePatterns and
            // mask exactly the class of hole this sweep exists to find.
            var name = $"Consumer{index:00}";
            var dir = Path.Combine(repo, "src", name);
            Directory.CreateDirectory(dir);

            WriteFile(Path.Combine(dir, name + ".csproj"), Csproj(d));
            WriteFile(Path.Combine(dir, "Support", "Marker.cs"), $$"""
                namespace Consumer{{index:00}};

                /// <summary>Keeps the project non-empty when its descriptor declares no shape.</summary>
                public sealed class SurfaceMarker
                {
                    public string Surface => "{{KeyOf(d)}}";
                }
                """);

            if (Shapes.TryGetValue(KeyOf(d), out var files))
                foreach (var (path, content) in files)
                    WriteFile(Path.Combine(dir, path.Replace('/', Path.DirectorySeparatorChar)), content);

            slnx.AppendLine($"    <Project Path=\"src/{name}/{name}.csproj\" />");
            projects.Add((d, name));
        }

        slnx.AppendLine("  </Folder>").AppendLine("</Solution>");
        WriteFile(Path.Combine(repo, "Coverage.slnx"), slnx.ToString());

        var snapshot = await AnalyzeAsync(repo);
        var signals = snapshot.Model.Architecture;

        var rows = ImmutableArray.CreateBuilder<Row>();
        foreach (var (d, name) in projects)
        {
            var fired = d.SignalKey.Length > 0 && signals.Has(d.SignalKey);
            var via = fired ? (signals.Get(d.SignalKey)?.DetectedVia ?? "?") : "-";

            var mine = snapshot.Entries.Where(e => InProject(e, name)).ToArray();
            var produced = d.Kind is { } k && mine.Any(e => e.Kind == k);
            var instead = mine.Length == 0
                ? "(no entries)"
                : string.Join(", ", mine.Select(e => e.Kind.ToString()).Distinct().OrderBy(s => s));

            rows.Add(new Row(KeyOf(d), name, d.SignalKey, d.Kind, fired, via, produced, instead));
        }

        return new SweepResult(rows.ToImmutable(), repo, snapshot.Entries.Length);
    }

    private static bool InProject(EntryPoint e, string project) =>
        string.Equals(e.Project, project, StringComparison.OrdinalIgnoreCase)
        || (e.Provenance?.Contains(project, StringComparison.OrdinalIgnoreCase) ?? false);

    private static string Csproj(EntrySurfaceDescriptor d)
    {
        var sdk = d.SdkHints.Length > 0 ? d.SdkHints[0] : "Microsoft.NET.Sdk";
        var sb = new StringBuilder();
        sb.AppendLine($"<Project Sdk=\"{sdk}\">");
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
        if (Props.TryGetValue(KeyOf(d), out var props))
            foreach (var (n, v) in props)
                sb.AppendLine($"    <{n}>{v}</{n}>");
        sb.AppendLine("  </PropertyGroup>");
        if (d.Packages.Length > 0)
        {
            sb.AppendLine("  <ItemGroup>");
            foreach (var pkg in d.Packages)
                sb.AppendLine($"    <PackageReference Include=\"{pkg}\" Version=\"1.0.0\" />");
            sb.AppendLine("  </ItemGroup>");
        }
        sb.AppendLine("</Project>");
        return sb.ToString();
    }

    private static void WriteFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static async Task<AnalysisSnapshot> AnalyzeAsync(string repoPath)
    {
        var fs = new RealFileSystem();
        var cache = new AnalysisCache(fs);
        var rootResult = await ProjectRootResolver.ResolveAsync(repoPath, fs, CancellationToken.None);
        var options = new ExtractionOptions { MaxOutputTokens = 8000, OutputFormat = OutputFormat.Markdown, AllowRoslyn = true };
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var ctx = new DiscoveryContext
        {
            RootPath = rootResult.EffectiveRootPath,
            ScopedProjectDirs = rootResult.ScopeProjectDirs,
            Options = options,
            ActiveScenario = ScenarioRegistry.BuiltIn["overview"],
            Observer = new NullDiscoveryObserver(),
            FileSystem = fs,
            Cache = cache,
            Analysis = new SharedAnalysisContext(),
            Logger = loggerFactory.CreateLogger("CatalogReachability"),
        };
        var pipeline = TestPipeline.Build(loggerFactory);
        return await pipeline.AnalyzeAsync(ctx);
    }

    // ── The allow-list (a ratchet, not a mute) ───────────────────────────────────────────────────

    private static Dictionary<string, string> Allowed()
    {
        var path = RepoFile("eval/expectations/catalog-reachability-allow.txt");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(path)) return map;
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var sep = line.IndexOf('=', StringComparison.Ordinal);
            Assert.True(sep > 0, $"Malformed allow-list line (want '<id> = <measured reason>'): {raw}");
            map[line[..sep].Trim()] = line[(sep + 1)..].Trim();
        }
        return map;
    }

    private static string RepoFile(string relative)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "DevContext.slnx")))
        {
            var parent = Path.GetDirectoryName(dir);
            if (parent == dir) break;
            dir = parent;
        }
        return Path.GetFullPath(Path.Combine(dir ?? Environment.CurrentDirectory, relative));
    }

    // ── The two properties ───────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Every_catalog_signal_is_reachable_from_a_consumer_app()
    {
        var sweep = await Sweep.Value;
        var allowed = Allowed();

        var unreachable = sweep.Rows
            .Where(r => r.SignalKey.Length > 0 && !r.SignalFired)
            .Select(r => $"signal:{r.SignalKey}")
            .ToArray();

        var undeclared = unreachable.Where(id => !allowed.ContainsKey(id)).ToArray();
        Assert.True(undeclared.Length == 0,
            $"""
            Catalog descriptors whose signal NO consumer app can fire (fixture repo: {sweep.RepoPath}):
              {string.Join("\n  ", undeclared)}
            Either give the descriptor a package/SDK/code-shape a consumer app can carry, or add the
            id to eval/expectations/catalog-reachability-allow.txt with the MEASURED reason.
            """);

        // Ratchet: a hole that is now closed must lose its allow-list line.
        var stale = allowed.Keys
            .Where(id => id.StartsWith("signal:", StringComparison.Ordinal) && !unreachable.Contains(id))
            .ToArray();
        Assert.True(stale.Length == 0,
            $"""
            These signals are now REACHABLE — delete their lines from
            eval/expectations/catalog-reachability-allow.txt (the file only ratchets down):
              {string.Join("\n  ", stale)}
            """);
    }

    [Fact]
    public async Task Every_catalog_kind_has_a_producing_path()
    {
        var sweep = await Sweep.Value;
        var allowed = Allowed();

        Assert.True(sweep.EntryCount > 0,
            $"The sweep produced NO entries at all — the instrument is dead, not the catalog. Repo: {sweep.RepoPath}");

        var missing = sweep.Rows
            .Where(r => r.Kind is not null && !r.KindProduced)
            .Select(r => (Id: $"kind:{r.Kind}@{(r.SignalKey.Length > 0 ? r.SignalKey : "-")}", r.Produced))
            .ToArray();

        var undeclared = missing.Where(m => !allowed.ContainsKey(m.Id)).ToArray();
        Assert.True(undeclared.Length == 0,
            $"""
            Catalog descriptors whose declared Kind their own consumer-app shape does NOT produce
            (fixture repo: {sweep.RepoPath}) — descriptor→signal→extractor→builder→entry is broken:
              {string.Join("\n  ", undeclared.Select(m => $"{m.Id}   produced instead: {m.Produced}"))}
            Close the path, or add the id to eval/expectations/catalog-reachability-allow.txt with the
            MEASURED reason.
            """);

        var stale = allowed.Keys
            .Where(id => id.StartsWith("kind:", StringComparison.Ordinal)
                && !missing.Any(m => m.Id == id))
            .ToArray();
        Assert.True(stale.Length == 0,
            $"""
            These kinds are now PRODUCED — delete their lines from
            eval/expectations/catalog-reachability-allow.txt (the file only ratchets down):
              {string.Join("\n  ", stale)}
            """);
    }

    // ── Named consumer-app shapes, each analysed in its OWN repo ─────────────────────────────────
    // The sweep above asks "can the catalog's declaration be reached at all". This asks the other
    // half: "does the shape a real consumer app writes reach the surface it should". Each case gets
    // an isolated one-project repo, because signals are repo-wide — put these in the big fixture and
    // a sibling project's signal would satisfy the extractor gate and hide the hole.

    private sealed record ShapeCase(
        string Id,
        string Sdk,
        string[] Packages,
        (string Name, string Value)[] Props,
        (string Path, string Content)[] Files,
        string? ExpectSignal,
        EntryPointKind? ExpectKind);

    private static readonly ShapeCase[] ShapeCases =
    [
        // A WinForms app that declares OutputType Exe rather than WinExe. UseWindowsForms is already
        // parsed onto ProjectInfo (CsprojReader.ParseUsesWinForms) and read by ArchitectureStyleDetector,
        // but the desktop-ui SIGNAL never looked at it.
        new("winforms-exe", "Microsoft.NET.Sdk", [],
            [("OutputType", "Exe"), ("UseWindowsForms", "true")],
            [
                ("MainForm.cs", """
                    using System.Windows.Forms;

                    namespace Consumer;

                    public partial class MainForm : Form
                    {
                        public MainForm() => InitializeComponent();

                        private void OnSaveClick(object sender, EventArgs e) { }
                    }
                    """),
                ("Program.cs", """
                    using System.Windows.Forms;

                    namespace Consumer;

                    internal static class Program
                    {
                        [STAThread]
                        private static void Main()
                        {
                            Application.EnableVisualStyles();
                            Application.Run(new MainForm());
                        }
                    }
                    """),
            ],
            ArchitectureSignals.Keys.DesktopUi, EntryPointKind.UiEntry),

        // A cross-platform Avalonia desktop head. Declared Exe, not WinExe — a WinExe-declared Avalonia
        // app is already caught by the generic "Microsoft.NET.Sdk + WinExe" rule, so the honest fixture
        // for the Avalonia-specific gap is the shape that gets nothing today.
        new("avalonia-exe", "Microsoft.NET.Sdk", ["Avalonia", "Avalonia.Desktop", "Avalonia.Themes.Fluent"],
            [("OutputType", "Exe")],
            [
                ("MainWindow.axaml.cs", """
                    using Avalonia.Controls;

                    namespace Consumer;

                    public partial class MainWindow : Window
                    {
                        public MainWindow() => InitializeComponent();
                    }
                    """),
                ("App.axaml.cs", """
                    using Avalonia;
                    using Avalonia.Controls.ApplicationLifetimes;

                    namespace Consumer;

                    public partial class App : Application
                    {
                        public override void OnFrameworkInitializationCompleted()
                        {
                            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                                desktop.MainWindow = new MainWindow();
                        }
                    }
                    """),
            ],
            ArchitectureSignals.Keys.DesktopUi, EntryPointKind.UiEntry),

        // A worker whose registration this repo never shows: Scrutor scanning, a library's own AddXxx()
        // extension, the Worker SDK's generated host, or a host composed outside the scan scope. There
        // is deliberately NO AddHostedService<T> anywhere in this fixture.
        new("hosted-service-unregistered", "Microsoft.NET.Sdk", [], [("OutputType", "Exe")],
            [
                ("Workers/InventorySyncWorker.cs", """
                    using Microsoft.Extensions.Hosting;

                    namespace Consumer.Workers;

                    public sealed class InventorySyncWorker : BackgroundService
                    {
                        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
                    }
                    """),
                ("ServiceRegistration.cs", """
                    using Microsoft.Extensions.DependencyInjection;

                    namespace Consumer;

                    public static class ServiceRegistration
                    {
                        // Scrutor: the worker is registered by assembly scan, never named.
                        public static IServiceCollection AddWorkers(this IServiceCollection services)
                            => services.Scan(scan => scan
                                .FromAssemblyOf<ServiceRegistration>()
                                .AddClasses(c => c.AssignableTo<Microsoft.Extensions.Hosting.IHostedService>())
                                .AsImplementedInterfaces()
                                .WithSingletonLifetime());
                    }
                    """),
            ],
            null, EntryPointKind.HostedService),
    ];

    public static TheoryData<string> ShapeIds()
    {
        var data = new TheoryData<string>();
        foreach (var c in ShapeCases) data.Add(c.Id);
        return data;
    }

    [Theory]
    [MemberData(nameof(ShapeIds))]
    public async Task Consumer_app_shape_reaches_its_surface(string id)
    {
        var shape = ShapeCases.Single(c => c.Id == id);
        var repo = Path.Combine(Path.GetTempPath(), "dc2-catalog-shapes", shape.Id);
        if (Directory.Exists(repo)) Directory.Delete(repo, recursive: true);
        var dir = Path.Combine(repo, "src", "ConsumerApp");
        Directory.CreateDirectory(dir);

        var sb = new StringBuilder();
        sb.AppendLine($"<Project Sdk=\"{shape.Sdk}\">");
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine("    <TargetFramework>net10.0</TargetFramework>");
        foreach (var (n, v) in shape.Props) sb.AppendLine($"    <{n}>{v}</{n}>");
        sb.AppendLine("  </PropertyGroup>");
        if (shape.Packages.Length > 0)
        {
            sb.AppendLine("  <ItemGroup>");
            foreach (var pkg in shape.Packages) sb.AppendLine($"    <PackageReference Include=\"{pkg}\" Version=\"1.0.0\" />");
            sb.AppendLine("  </ItemGroup>");
        }
        sb.AppendLine("</Project>");
        WriteFile(Path.Combine(dir, "ConsumerApp.csproj"), sb.ToString());
        foreach (var (path, content) in shape.Files)
            WriteFile(Path.Combine(dir, path.Replace('/', Path.DirectorySeparatorChar)), content);
        WriteFile(Path.Combine(repo, "ConsumerApp.slnx"),
            "<Solution>\n  <Folder Name=\"/src/\">\n    <Project Path=\"src/ConsumerApp/ConsumerApp.csproj\" />\n  </Folder>\n</Solution>\n");

        var snapshot = await AnalyzeAsync(repo);
        var signalOk = shape.ExpectSignal is null || snapshot.Model.Architecture.Has(shape.ExpectSignal);
        var kindOk = shape.ExpectKind is null || snapshot.Entries.Any(e => e.Kind == shape.ExpectKind);
        var reached = signalOk && kindOk;

        var allowed = Allowed();
        var allowId = $"shape:{shape.Id}";
        if (allowed.ContainsKey(allowId))
        {
            Assert.False(reached,
                $"""
                Shape '{shape.Id}' now reaches its surface — delete its line from
                eval/expectations/catalog-reachability-allow.txt (the file only ratchets down).
                """);
            return;
        }

        Assert.True(reached,
            $"""
            Consumer-app shape '{shape.Id}' does not reach its surface (repo: {repo}):
              signal {shape.ExpectSignal ?? "-"}: {(signalOk ? "fired" : "MISSING")}
              kind   {shape.ExpectKind?.ToString() ?? "-"}: {(kindOk ? "produced" : "MISSING")}
              entries produced: {(snapshot.Entries.Length == 0 ? "(none)" : string.Join(", ", snapshot.Entries.Select(e => $"{e.Kind}:{e.Title}")))}
            Close the path, or add {allowId} to eval/expectations/catalog-reachability-allow.txt with
            the MEASURED reason.
            """);
    }

    /// <summary>
    /// The sweep's own report. Not an assertion — it is how a session reads what the catalog can and
    /// cannot reach without re-deriving it, and it is the evidence artifact D1.2 diffs against.
    /// </summary>
    [Fact]
    public async Task Report_the_reachability_matrix()
    {
        var sweep = await Sweep.Value;
        var sb = new StringBuilder();
        sb.AppendLine($"repo: {sweep.RepoPath}");
        sb.AppendLine($"entries: {sweep.EntryCount}");
        sb.AppendLine();
        sb.AppendLine("| descriptor | project | signal | via | kind | produced |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (var r in sweep.Rows)
            sb.AppendLine($"| {r.Key} | {r.Project} | {(r.SignalKey.Length == 0 ? "-" : r.SignalFired ? "FIRED" : "**MISSING**")} | {r.Via} | {r.Kind?.ToString() ?? "-"} | {(r.Kind is null ? "-" : r.KindProduced ? "yes" : $"**no** ({r.Produced})")} |");

        var outPath = Path.Combine(sweep.RepoPath, "reachability-matrix.md");
        File.WriteAllText(outPath, sb.ToString());
        Assert.True(File.Exists(outPath));
    }
}
