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
            var result = await dialog.ShowDialog<bool>(this);
            
            if (result == true)
            {
                var projectViewModel = dialog.ViewModel;
                
                // Створюємо новий проєкт на основі введених даних
                var newProject = new MaterialFlow.Models.VideoProject
                {
                    Name = projectViewModel.ProjectName,
                    SourceFilePath = projectViewModel.SourceFilePath,
                    CreatedAt = System.DateTime.UtcNow
                };

                // Додаємо його в список проєктів на головному екрані
                _viewModel.Projects.Insert(0, newProject);

                // Створюємо пресет на основі вибраних налаштувань
                var preset = new MaterialFlow.Models.Preset
                {
                    Name = projectViewModel.SelectedPlatform,
                    Resolution = projectViewModel.SelectedResolution,
                    // Parse bitrate if it's a number, otherwise default
                    Bitrate = int.TryParse(projectViewModel.SelectedBitrate, out int b) ? b : 5000,
                    Codec = "libx264"
                };

                // Запускаємо конвертацію (асинхронно у фоні), передаємо SavePath як шлях для експорту
                _ = _viewModel.StartConversionAsync(newProject, preset, projectViewModel.SavePath);
            }
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