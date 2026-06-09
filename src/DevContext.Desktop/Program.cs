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

        // Read HTML and inject JS inline so everything works from LoadRawString
        var htmlPath = Path.Combine(wwwroot, "index.html");
        var jsPath = Path.Combine(wwwroot, "app.js");
        if (File.Exists(htmlPath))
        {
            var html = File.ReadAllText(htmlPath);
            if (File.Exists(jsPath))
            {
                var js = File.ReadAllText(jsPath);
                html = html.Replace("<script src=\"app.js\"></script>",
                    "<script>" + js + "</script>");
            }
            window.LoadRawString(html);
        }
        else
        {
            window.LoadRawString("<h1 style='color:white'>404 - index.html not found</h1>");
        }

        window.WaitForClose();
    }
}
