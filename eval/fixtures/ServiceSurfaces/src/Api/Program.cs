using Api.Rpc;
using Api.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddScoped<IGreetingService, GreetingService>();
builder.Services.AddScoped<IOrderIngestService, OrderIngestService>();
builder.Services.AddScoped<ICatalogLookupService, CatalogLookupService>();

var app = builder.Build();
app.MapGrpcService<GreeterService>();
app.Run();
