using System.Windows;

namespace DevContext.Desktop;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new Application();
        app.Run(new MainWindow());
    }
}
