using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MaterialFlow.ViewModels;
using System.Threading.Tasks;

namespace MaterialFlow
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel = new MainWindowViewModel();
        }

        private async void CreateProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new Views.CreateProjectWindow();
            await dialog.ShowDialog(this);
        }

        private void ToggleSidebar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _viewModel.ToggleSidebar();
        }

        private async void Login_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new Views.LoginWindow();
            var result = await dialog.ShowDialog<bool>(this);
            if (result)
            {
                _viewModel.CurrentUser = Services.AuthService.Instance.CurrentUser;
            }
        }

        private void Logout_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _viewModel.Logout();
        }

        private async void SelectDefaultPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Default Projects Folder",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                _viewModel.DefaultSavePath = folders[0].Path.LocalPath;
            }
        }
    }
}