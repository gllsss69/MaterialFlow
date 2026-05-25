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
                
                int parsedBitrate = 5000;
                if (projectViewModel.SelectedBitrate != "Auto")
                {
                    var bitRateStr = projectViewModel.SelectedBitrate.Replace(" Mbps", "").Replace(" kbps", "");
                    if (int.TryParse(bitRateStr, out int b))
                    {
                        parsedBitrate = projectViewModel.SelectedBitrate.Contains("Mbps") ? b * 1000 : b;
                    }
                }

                int parsedFps = 30;
                var fpsStr = projectViewModel.SelectedFPS.Replace(" fps", "");
                if (int.TryParse(fpsStr, out int f))
                {
                    parsedFps = f;
                }

                string selectedCodec = projectViewModel.SelectedCodec;
                if (selectedCodec == "Auto")
                {
                    selectedCodec = projectViewModel.SelectedFormat.TrimStart('.').Equals("avi", System.StringComparison.OrdinalIgnoreCase) ? "mpeg4" : "libx264";
                }

                // Створюємо новий проєкт на основі введених даних
                var newProject = new MaterialFlow.Models.VideoProject
                {
                    Name = projectViewModel.ProjectName,
                    SourceFilePath = projectViewModel.SourceFilePath,
                    Resolution = projectViewModel.SelectedResolution,
                    Bitrate = projectViewModel.SelectedBitrate == "Auto" ? "5000 kbps" : projectViewModel.SelectedBitrate,
                    Format = projectViewModel.SelectedFormat.TrimStart('.'),
                    FPS = projectViewModel.SelectedFPS,
                    Codec = selectedCodec,
                    UseWatermark = projectViewModel.UseWatermark,
                    CreatedAt = System.DateTime.UtcNow
                };

                // Додаємо його в список проєктів на головному екрані
                _viewModel.Projects.Insert(0, newProject);

                // Створюємо пресет на основі вибраних налаштувань
                var preset = new MaterialFlow.Models.Preset
                {
                    Name = projectViewModel.SelectedPlatform,
                    Resolution = projectViewModel.SelectedResolution,
                    Bitrate = parsedBitrate,
                    FrameRate = parsedFps,
                    Codec = selectedCodec
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

        private async void RenameProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Models.VideoProject project)
            {
                var dialog = new Views.RenameWindow(project.Name);
                var newName = await dialog.ShowDialog<string>(this);
                if (!string.IsNullOrWhiteSpace(newName) && newName != project.Name)
                {
                    var oldSafeName = string.Join("_", project.Name.Split(System.IO.Path.GetInvalidFileNameChars()));
                    var oldDir = System.IO.Path.Combine(_viewModel.DefaultSavePath, oldSafeName);
                    
                    project.Name = newName;
                    
                    var newSafeName = string.Join("_", project.Name.Split(System.IO.Path.GetInvalidFileNameChars()));
                    var newDir = System.IO.Path.Combine(_viewModel.DefaultSavePath, newSafeName);

                    if (System.IO.Directory.Exists(oldDir) && !System.IO.Directory.Exists(newDir))
                    {
                        try
                        {
                            System.IO.Directory.Move(oldDir, newDir);
                        }
                        catch { }
                    }

                    if (!string.IsNullOrEmpty(project.ThumbnailPath) && project.ThumbnailPath.Contains(oldSafeName))
                    {
                        project.ThumbnailPath = project.ThumbnailPath.Replace(oldSafeName, newSafeName);
                    }
                    if (!string.IsNullOrEmpty(project.ExportFilePath) && project.ExportFilePath.Contains(oldSafeName))
                    {
                        project.ExportFilePath = project.ExportFilePath.Replace(oldSafeName, newSafeName);
                    }

                    _viewModel.UpdateProject(project);
                }
            }
        }

        private async void PropertiesProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Models.VideoProject project)
            {
                var dialog = new Views.PropertiesWindow(project);
                await dialog.ShowDialog(this);
            }
        }

        private void DeleteProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Models.VideoProject project)
            {
                _viewModel.Projects.Remove(project);
                // Also remove directory
                var safeProjectName = string.Join("_", project.Name.Split(System.IO.Path.GetInvalidFileNameChars()));
                var projectDir = System.IO.Path.Combine(_viewModel.DefaultSavePath, safeProjectName);
                if (System.IO.Directory.Exists(projectDir))
                {
                    try
                    {
                        System.IO.Directory.Delete(projectDir, true);
                    }
                    catch { }
                }
            }
        }

        private void OpenVideo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Models.VideoProject project)
            {
                var filePath = project.ExportFilePath;
                if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true // Opens with default video player
                        });
                    }
                    catch { }
                }
            }
        }
    }
}