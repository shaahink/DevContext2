using CompositionApp.Web.Hubs;

namespace CompositionApp.Web.Configuration;

/// <summary>
/// The HTTP pipeline — including the mapped SignalR hub — is composed in an extension method, not in
/// Program.cs. The engine must walk this method to see MapHub&lt;PriceHub&gt;.
/// </summary>
public static class MiddlewarePipeline
{
    public static WebApplication UseCompositionApp(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<PriceHub>("/hubs/price");
        return app;
    }
}
