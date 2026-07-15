var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// FeedsService is intentionally NOT registered explicitly here — DntSite auto-registers services by
// convention, so interface→impl must resolve from the type graph, not from a DI registration.

var app = builder.Build();
app.MapControllers();
app.Run();
