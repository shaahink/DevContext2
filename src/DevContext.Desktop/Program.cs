using Photino.NET;
using DevContext.Desktop.Bridge;

namespace DevContext.Desktop;

public static class Program
{
    public static void Main(string[] args)
    {
        var baseDir = AppContext.BaseDirectory;
        var wwwroot = Path.Combine(baseDir, "wwwroot");
        if (!Directory.Exists(wwwroot))
            wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var window = new PhotinoWindow()
            .SetTitle("DevContext")
            .SetUseOsDefaultSize(false)
            .SetSize(1280, 800)
            .SetMinSize(600, 450)
            .Center()
            .SetResizable(true)
            .SetDevToolsEnabled(true);

        var bridge = new DevContextBridge(window);

        window.RegisterWebMessageReceivedHandler((sender, message) =>
        {
            bridge.HandleMessage(message);
        });

        // Load from file:// URL for proper script/CDN support
        var htmlPath = Path.Combine(wwwroot, "index.html");
        if (File.Exists(htmlPath))
        {
            window.Load($"file:///{htmlPath.Replace('\\', '/')}");
        }
        else
        {
            window.LoadRawString("<h1>404 - index.html not found</h1>");
        }
        window.WaitForClose();
    }
}
