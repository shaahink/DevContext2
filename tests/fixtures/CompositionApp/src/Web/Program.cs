using CompositionApp.Web.Configuration;

// Program.cs composes the app entirely through extension methods — the shamshir shape where NO
// registrations, hosted services, hubs or middleware are visible from Program.cs alone. The engine
// must follow the extension methods to see the workers, SignalR and the mapped hub.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCompositionApp(builder.Configuration);

var app = builder.Build();

app.UseCompositionApp();

app.Run();
