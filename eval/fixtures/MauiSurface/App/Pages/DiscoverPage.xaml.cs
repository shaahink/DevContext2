namespace MauiSurface.Pages;

/// <summary>Fixture (Prism D1.2b / audit B2): the ContentPage shape dotnet-podcasts uses —
/// XAML plus code-behind, constructor-injected view model.</summary>
public partial class DiscoverPage : ContentPage
{
    private readonly ViewModels.DiscoverViewModel viewModel;

    public DiscoverPage(ViewModels.DiscoverViewModel vm)
    {
        InitializeComponent();
        BindingContext = viewModel = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Load();
    }
}
