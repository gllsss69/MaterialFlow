using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Layout;
using Avalonia.Layout;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly FFmpegService _ffmpegService;
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set 
            { 
                _currentUser = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsLoggedOut));
                OnPropertyChanged(nameof(UserFullName));
                OnPropertyChanged(nameof(UserInitials));
            }
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsLoggedOut => CurrentUser == null;
        public string UserFullName => CurrentUser?.FullName ?? "Guest";
        public string UserInitials => string.IsNullOrWhiteSpace(CurrentUser?.FullName) ? "?" : 
            new string(CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToArray()).ToUpper();

        private bool _isSidebarCollapsed;
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set
            {
                _isSidebarCollapsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SidebarWidth));
                OnPropertyChanged(nameof(DividerWidth));
                OnPropertyChanged(nameof(ItemWidth));
                OnPropertyChanged(nameof(SidebarContentAlignment));
            }
        }

        public double SidebarWidth => IsSidebarCollapsed ? 104 : 280;
        public double DividerWidth => IsSidebarCollapsed ? 56 : 232;
        public double ItemWidth => IsSidebarCollapsed ? 80 : 232;
        public double ItemHeight => 56;
        public HorizontalAlignment SidebarContentAlignment => IsSidebarCollapsed ? HorizontalAlignment.Center : HorizontalAlignment.Left;

        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set { _currentPageIndex = value; OnPropertyChanged(); }
        }

        // Settings
        private string _selectedLanguage = "English";
        private string _selectedTheme = "System Default";
        private string _defaultSavePath = "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(); }
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set { _selectedTheme = value; OnPropertyChanged(); }
        }

        public string DefaultSavePath
        {
            get => _defaultSavePath;
            set 
            { 
                _defaultSavePath = value; 
                OnPropertyChanged(); 
                LoadProjects();
            }
        }

        public ObservableCollection<string> Languages { get; } = new() { "English", "Ukrainian", "German", "French" };
        public ObservableCollection<string> Themes { get; } = new() { "System Default", "Light", "Dark" };

        public void Logout()
        {
            AuthService.Instance.Logout();
            CurrentUser = null;
        }

        public void SetPageIndex(string index)
        {
            if (int.TryParse(index, out int i))
            {
                CurrentPageIndex = i;
            }
        }

        public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

        public ObservableCollection<VideoProject> Projects { get; set; } = new();
        public ObservableCollection<ConversionJob> ConversionJobs { get; set; } = new();

    public MainWindowViewModel()
    {
        // Вкажіть шлях до ffmpeg (для демонстрації припускаємо, що він в PATH або в тій же папці)
        _ffmpegService = new FFmpegService("ffmpeg");
        LoadProjects();
    }

    private void LoadProjects()
    {
        Projects.Clear();
        if (!Directory.Exists(DefaultSavePath)) return;

        try
        {
            var directories = Directory.GetDirectories(DefaultSavePath);
            foreach (var dir in directories)
            {
                var projectFile = Path.Combine(dir, "project.json");
                if (File.Exists(projectFile))
                {
                    var json = File.ReadAllText(projectFile);
                    var project = JsonSerializer.Deserialize<VideoProject>(json);
                    if (project != null)
                    {
                        project.IsProcessing = false;
                        // If progress is 100, the project is completed successfully (no status text). Otherwise, it's considered an Error.
                        project.StatusText = project.Progress >= 100 ? "" : "Error";
                        Projects.Add(project);
                    }
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Запуск завдання конвертації відео
    /// </summary>
    public async Task StartConversionAsync(VideoProject project, Preset preset, string exportPath)
    {
        if (project == null || preset == null) return;

        // Створюємо папку для проєкту
        var safeProjectName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
        var projectDir = Path.Combine(DefaultSavePath, safeProjectName);
        
        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
        }

        // Зберігаємо метадані проєкту
        try
        {
            var projectJson = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectDir, "project.json"), projectJson);
        }
        catch { }

        // Визначаємо шлях для експорту (якщо не вказаний, зберігаємо в папці проєкту)
        if (string.IsNullOrEmpty(exportPath))
        {
            exportPath = projectDir;
        }

        var job = new ConversionJob
        {
            ProjectId = project.Id,
            PresetId = preset.Id,
            OutputPath = Path.Combine(exportPath, $"{project.Name}_{preset.Name}.mp4")
        };

        // Переконуємось, що папка для експорту існує
        var exportDir = Path.GetDirectoryName(job.OutputPath);
        if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        ConversionJobs.Add(job);

        // Налаштовуємо Progress для оновлення UI
        project.IsProcessing = true;
        project.StatusText = "Processing...";
        project.Progress = 0;

        var progress = new Progress<double>(p =>
        {
            job.Progress = p;
            project.Progress = p;
            project.StatusText = $"Processing: {p:F1}%";
        });

        // Запускаємо конвертацію
        bool success = await _ffmpegService.ConvertVideoAsync(project, preset, job, progress);

        if (success)
        {
            project.IsProcessing = false;
            project.StatusText = ""; // Immediately empty on success
            project.Progress = 100;
        }
        else
        {
            project.IsProcessing = false;
            project.StatusText = "Error"; // Persists on failure
        }

        // Перезаписуємо project.json з оновленим статусом
        try
        {
            var projectJson = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectDir, "project.json"), projectJson);
        }
        catch { }

        OnPropertyChanged(nameof(ConversionJobs));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
