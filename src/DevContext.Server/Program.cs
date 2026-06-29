using DevContext.Server.Endpoints;
using DevContext.Server.Sessions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<EngineHostCache>();

var serverOptions = new ServerOptions();
builder.Configuration.GetSection("DevContext").Bind(serverOptions);
builder.Services.AddSingleton(serverOptions);

builder.Services.AddSingleton<IEngineRunner, EngineRunner>();
builder.Services.AddSingleton<IAnalysisSessionManager, AnalysisSessionManager>();

builder.Services.AddGrpc();

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

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.UseCors("web");

app.MapGrpcService<DevContextGrpcService>().RequireCors("web");

app.MapGet("/health", () => Results.Ok(new { status = "ok", version = DevContext.Core.DevContextVersion.Display }));

app.Run();

/// <summary>Exposed so the integration test host can boot the real composition root.</summary>
public partial class Program;
