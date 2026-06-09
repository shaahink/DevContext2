using CommunityToolkit.Mvvm.ComponentModel;

namespace DevContext.Desktop.ViewModels;

public enum TokenWeight { Light, Medium, Heavy, Critical }

public partial class SectionViewModel : ObservableObject
{
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string FullText { get; init; } = "";

    [ObservableProperty]
    private bool _isIncluded = true;

    [ObservableProperty]
    private int _rawTokens;

    [ObservableProperty]
    private int _compressedTokens;

    public TokenWeight Weight => CompressedTokens switch
    {
        >= 1000 => TokenWeight.Critical,
        >= 500 => TokenWeight.Heavy,
        >= 100 => TokenWeight.Medium,
        _ => TokenWeight.Light,
    };

    public bool WasTruncated { get; init; }
    public string? TruncationReason { get; init; }
}

public partial class SectionGroupViewModel : ObservableObject
{
    public string Name { get; init; } = "";

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isIncluded = true;

    public List<SectionViewModel> Children { get; } = [];

    public int TotalTokens => Children.Sum(c => c.CompressedTokens);

    partial void OnIsIncludedChanged(bool value)
    {
        foreach (var child in Children)
            child.IsIncluded = value;
    }
}
