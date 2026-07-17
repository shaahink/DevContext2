namespace MauiSurface.ViewModels;

/// <summary>Fixture (Prism D1.2b / audit B2): the MVVM view model behind DiscoverPage.
/// Its [RelayCommand] handler is a UI entry point too.</summary>
public sealed class DiscoverViewModel
{
    public string Title { get; private set; } = "";

    public void Load() => Title = "Discover";

    [RelayCommand]
    private void OpenShow(string showId) => Title = showId;
}
