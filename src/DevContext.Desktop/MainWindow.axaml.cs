using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DevContext.Desktop.ViewModels;

namespace DevContext.Desktop;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private MainViewModel? VM => DataContext as MainViewModel;

    private void OnWindowOpened(object? sender, EventArgs e)
    {
        if (VM is not { } vm) return;

        SyncSegmentedControl(ProfileListBox, vm.SelectedProfile);
        SyncSegmentedControl(FormatListBox, vm.SelectedFormat);
    }

    private void OnProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0
            && e.AddedItems[0] is ListBoxItem { Tag: string profile }
            && VM is { } vm)
        {
            vm.SetProfileCommand.Execute(profile);
        }
    }

    private void OnFormatSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0
            && e.AddedItems[0] is ListBoxItem { Tag: string format }
            && VM is { } vm)
        {
            vm.SetFormatCommand.Execute(format);
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5 && VM?.AnalyzeCommand.CanExecute(null) == true)
        {
            VM.AnalyzeCommand.Execute(null);
            e.Handled = true;
        }
    }

    private static void SyncSegmentedControl(ListBox listBox, string value)
    {
        foreach (var item in listBox.Items.OfType<ListBoxItem>())
        {
            if (item.Tag is string tag && tag == value)
            {
                listBox.SelectedItem = item;
                return;
            }
        }
    }

    private async void OnPickFolder(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select .NET project folder",
            AllowMultiple = false,
        });
        if (folders.Count > 0 && VM is { } vm)
            vm.ProjectPath = folders[0].Path.LocalPath;
    }

    private async void OnPickFile(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select solution or project file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Solution / Project")
                {
                    Patterns = ["*.sln", "*.slnx", "*.csproj"]
                }
            ],
        });
        if (files.Count > 0 && VM is { } vm)
            vm.ProjectPath = files[0].Path.LocalPath;
    }

    private async void OnCopy(object? sender, RoutedEventArgs e)
    {
        if (VM is { RawContent: { Length: > 0 } text } && Clipboard is { } cb)
            await cb.SetTextAsync(text);
    }

    private async void OnCopyLlm(object? sender, RoutedEventArgs e)
    {
        if (VM is { } vm && Clipboard is { } cb)
            await cb.SetTextAsync(vm.LlmViewText);
    }

    private void OnSwitchToHuman(object? sender, RoutedEventArgs e)
    {
        if (VM is { } vm) vm.IsHumanView = true;
    }

    private void OnSwitchToLlm(object? sender, RoutedEventArgs e)
    {
        if (VM is { } vm) vm.IsHumanView = false;
    }

    private async void OnPasteGitHub(object? sender, RoutedEventArgs e)
    {
        if (Clipboard is { } cb)
        {
            var text = await cb.GetTextAsync();
            if (!string.IsNullOrWhiteSpace(text) && VM is { } vm)
                vm.ProjectPath = text.Trim();
        }
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        if (VM is not { RawContent: { Length: > 0 } content } vm) return;

        var ext = vm.SelectedFormat == "json" ? "json" : "md";
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save output",
            DefaultExtension = ext,
            FileTypeChoices = ext == "json"
                ? [new FilePickerFileType("JSON") { Patterns = ["*.json"] }]
                : [new FilePickerFileType("Markdown") { Patterns = ["*.md"] }],
        });

        if (file is not null)
            await File.WriteAllTextAsync(file.Path.LocalPath, content);
    }
}
