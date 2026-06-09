using CommunityToolkit.Mvvm.ComponentModel;

namespace DevContext.Desktop.ViewModels;

public partial class SectionViewModel : ObservableObject
{
    public string Name { get; init; } = "";
    public string FullText { get; init; } = "";

    [ObservableProperty]
    private bool _isIncluded = true;

    [ObservableProperty]
    private int _rawTokens;

    [ObservableProperty]
    private int _compressedTokens;

    public bool WasTruncated { get; init; }
    public string? TruncationReason { get; init; }
}
