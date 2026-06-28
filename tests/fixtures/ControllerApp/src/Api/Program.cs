using Core;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

app.MapControllers();

app.Run();
