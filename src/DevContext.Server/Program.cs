using DevContext.Server.Endpoints;
using DevContext.Server.Services;
using DevContext.Server.Sessions;
using DevContext.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<EngineHostCache>();
builder.Services.AddSingleton<CloneRegistry>();

var serverOptions = new ServerOptions();
builder.Configuration.GetSection("DevContext").Bind(serverOptions);

// DEVCONTEXT_ENDPOINT (T1.4): the port is otherwise hardcoded on BOTH sides, so two checkouts on
// one machine share 127.0.0.1:5179 — and ServerShim reuses whatever already answers /health there.
// Measured 2026-08-13: a wire probe in this repo was served by ANOTHER repo's build and reported
// this repo's just-landed fixes as still broken. The MCP reads the same variable and the server it
// spawns inherits it, so one env var isolates a whole probe/gate run.
//
// MEASURED, not read off the code below: ServerOptions.Urls carries a non-empty DEFAULT, so the
// `if (string.IsNullOrEmpty(serverOptions.Urls))` further down can never fire. The only way to tell
// "nobody configured a URL" from "somebody configured 5179" is to ask the configuration directly —
// which also keeps explicit config winning over the variable.
var configuredUrls = builder.Configuration["DevContext:Urls"];
var endpointOverride = Environment.GetEnvironmentVariable("DEVCONTEXT_ENDPOINT");
if (string.IsNullOrEmpty(configuredUrls) && !string.IsNullOrEmpty(endpointOverride))
    serverOptions = serverOptions with { Urls = endpointOverride };

builder.Services.AddSingleton(serverOptions);

builder.Services.AddSingleton<IEngineRunner, EngineRunner>();
builder.Services.AddSingleton<IAnalysisSessionManager, AnalysisSessionManager>();

// M3.3 — MCP observability service
builder.Services.AddSingleton<McpObservabilityService>();

builder.Services.AddGrpc();
// T6.10 — RecordToolCall reads the request content-type to tag UI vs agent origin.
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options => options.AddPolicy("web", policy => policy
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("grpc-status", "grpc-message", "grpc-encoding", "grpc-accept-encoding")));

if (string.IsNullOrEmpty(serverOptions.Urls))
    serverOptions = serverOptions with { Urls = "http://127.0.0.1:5179" };

if (string.IsNullOrEmpty(builder.Configuration["urls"]))
    builder.WebHost.UseUrls(serverOptions.Urls);

var app = builder.Build();

// F5 (Prism D4.5) — tag the ui/agent origin for this request. N4.1: the tag is read off the
// CLIENT's headers now, not the framing — both the app and the MCP sidecar speak gRPC-web, so
// the old content-type sniff called every agent call "ui" (see OriginTag).
app.Use((context, next) =>
{
    context.Items[DevContext.Server.Endpoints.OriginTag.ItemKey] =
        DevContext.Server.Endpoints.OriginTag.FromRequest(
            context.Request.Headers.UserAgent,
            context.Request.Headers["x-user-agent"],
            context.Request.Headers.Origin);
    return next(context);
});
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors("web");

app.MapGrpcService<DevContextGrpcService>().RequireCors("web");

// /health says WHICH BUILD is answering, not just that something is (T1.4).
// version alone is "1.0.0" in every checkout, so it cannot tell two engines apart. baseDirectory
// and startedAt can: a probe asserts the answer comes from ITS repo, and that the process started
// AFTER the last build of the dll it is about to measure — the stale-binary trap this program has
// paid for repeatedly, caught by the gate instead of by a confusing red three sessions later.
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    version = DevContext.Core.DevContextVersion.Display,
    baseDirectory = AppContext.BaseDirectory,
    pid = Environment.ProcessId,
    startedAt = System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()
        .ToString("o", System.Globalization.CultureInfo.InvariantCulture),
}));

app.Run();

/// <summary>Exposed so the integration test host can boot the real composition root.</summary>
public partial class Program;
