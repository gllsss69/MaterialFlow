using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MaterialFlow.ViewModels;
using System.Threading.Tasks;

namespace MaterialFlow.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private async void SelectDefaultPath_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Default Projects Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0 && DataContext is MainWindowViewModel vm)
        {
            vm.DefaultSavePath = folders[0].Path.LocalPath;
        }
    }

    private void OpenGitHub_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/gllsss69/MaterialFlow",
                UseShellExecute = true
            });
        }
        catch { }
    }
}
