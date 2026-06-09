using Photino.NET;
using DevContext.Desktop.Bridge;

namespace DevContext.Desktop;

public static class Program
{
    public static void Main(string[] args)
    {
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

        window.Load("wwwroot/index.html");
        window.WaitForClose();
    }
}
