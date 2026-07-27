using DevContext.Server.Endpoints;
using DevContext.Server.Services;
using DevContext.Server.Sessions;
using DevContext.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<EngineHostCache>();
builder.Services.AddSingleton<CloneRegistry>();

var serverOptions = new ServerOptions();
builder.Configuration.GetSection("DevContext").Bind(serverOptions);
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

// F5 (Prism D4.5) — tag the ui/agent origin BEFORE UseGrpcWeb rewrites the content-type
// to plain application/grpc (post-rewrite sniffing labeled the app's own RPCs "agent").
app.Use((context, next) =>
{
    context.Items[DevContext.Server.Endpoints.OriginTag.ItemKey] =
        DevContext.Server.Endpoints.OriginTag.FromContentType(context.Request.ContentType);
    return next(context);
});
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors("web");

app.MapGrpcService<DevContextGrpcService>().RequireCors("web");

app.MapGet("/health", () => Results.Ok(new { status = "ok", version = DevContext.Core.DevContextVersion.Display }));

app.Run();

/// <summary>Exposed so the integration test host can boot the real composition root.</summary>
public partial class Program;
