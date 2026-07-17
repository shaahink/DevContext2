namespace MauiSurface;

/// <summary>Fixture (Prism D1.2b / audit B2): a Shell subclass — the app's navigation root.
/// It must render as a UI entry point.</summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("player", typeof(Pages.PlayerPage));
    }
}
