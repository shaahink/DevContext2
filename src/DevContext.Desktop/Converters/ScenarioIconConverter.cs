using Avalonia.Data.Converters;

namespace DevContext.Desktop.Converters;

public class ScenarioIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string scenario)
            return App.Current?.Resources["IconHelpCircle"];

        var key = scenario switch
        {
            "architecture" => "IconSitemap",
            "debug-endpoint" => "IconMagnify",
            "add-similar-feature" => "IconPlusCircle",
            "modify-middleware" => "IconCog",
            "trace-message-flow" => "IconMessage",
            "harden-di" => "IconShieldCheck",
            _ => "IconHelpCircle",
        };

        return App.Current?.Resources[key];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => null;
}
