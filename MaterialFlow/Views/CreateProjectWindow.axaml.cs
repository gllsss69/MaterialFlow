using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MaterialFlow.ViewModels;
using System.Threading.Tasks;

namespace MaterialFlow.Views;

public partial class CreateProjectWindow : Window
{
    private CreateProjectViewModel _viewModel;
    public CreateProjectViewModel ViewModel => _viewModel;

    public CreateProjectWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new CreateProjectViewModel();
    }

    private async void SelectSourceFile_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Source Video",
            AllowMultiple = false,
            FileTypeFilter = new[] 
            { 
                new FilePickerFileType("Video files") 
                { 
                    Patterns = new[] { "*.mp4", "*.mkv", "*.avi", "*.mov" } 
                } 
            }
        });

        if (files.Count > 0)
        {
            _viewModel.SourceFilePath = files[0].TryGetLocalPath() ?? files[0].Path.ToString();
            _viewModel.SourceFileName = files[0].Name;
            _viewModel.IsSourceFileSelected = true;
        }
    }

    private async void SelectPath_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Destination Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            _viewModel.SavePath = folders[0].TryGetLocalPath() ?? folders[0].Path.ToString();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void Create_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Validate())
        {

            Close(true);
        }
    }
}
