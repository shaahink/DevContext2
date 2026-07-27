var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<BlazorSurface.Data.ShowService>();
var app = builder.Build();
app.Run();
