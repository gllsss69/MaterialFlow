using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Поточна вибрана мова локалізації інтерфейсу програми.
        /// При зміні оновлює мовний ресурс та зберігає налаштування у файл.
        /// </summary>
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set 
            { 
                _selectedLanguage = value; 
                OnPropertyChanged(); 
                ApplyLanguage(value);
                OnPropertyChanged(nameof(SelectedSortText));
                OnPropertyChanged(nameof(FilteredProjects));
                SaveSettings();
            }
        }

        /// <summary>
        /// Динамічно завантажує та застосовує словник ресурсів локалізації (.axaml)
        /// відповідно до обраної мови користувача.
        /// </summary>
        /// <param name="languageName">Назва обраної мови (наприклад, "Ukrainian", "English").</param>
        private void ApplyLanguage(string languageName)
        {
            if (Avalonia.Application.Current == null) return;
            
            string langCode = languageName switch
            {
                "Czech" => "cz",
                "German" => "de",
                "Japanese" => "ja",
                "Polish" => "pl",
                "Slovak" => "sk",
                "Ukrainian" => "uk",
                _ => "en"
            };

            var uri = new Uri($"avares://MaterialFlow/Resources/Langs/{langCode}.axaml");
            try
            {
                var newDict = (Avalonia.Controls.ResourceDictionary)Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(uri);
                
                var mergedDicts = Avalonia.Application.Current.Resources.MergedDictionaries;
                var existingDict = mergedDicts.OfType<Avalonia.Controls.ResourceDictionary>().FirstOrDefault(d => d.ContainsKey("NavHome"));
                
                if (existingDict != null)
                {
                    mergedDicts.Remove(existingDict);
                }
                
                mergedDicts.Add(newDict);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language {langCode}: {ex.Message}");
            }
        }

        /// <summary>
        /// Поточна вибрана тема оформлення інтерфейсу (Світла, Темна або Системна).
        /// При зміні оновлює колірну схему вікон та зберігає налаштування.
        /// </summary>
        public string SelectedTheme
        {
            get => _selectedTheme;
            set 
            { 
                _selectedTheme = value; 
                OnPropertyChanged(); 
                ApplyTheme(value);
                SaveSettings();
            }
        }

        /// <summary>
        /// Застосовує вказану тему оформлення до поточного застосунку Avalonia.
        /// </summary>
        /// <param name="theme">Назва теми ("Dark", "Light" або "System Default").</param>
        private void ApplyTheme(string theme)
        {
            if (Avalonia.Application.Current != null)
            {
                if (theme == "Dark")
                {
                    Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                }
                else if (theme == "Light")
                {
                    Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                }
                else
                {
                    Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;
                }
                                var materialTheme = Avalonia.Application.Current.Styles.OfType<Material.Styles.Themes.MaterialTheme>().FirstOrDefault();
                if (materialTheme != null)
                {
                    var prop = materialTheme.GetType().GetProperty("BaseTheme");
                    if (prop != null)
                    {
                        var propType = prop.PropertyType;
                        try
                        {
                            if (theme == "Dark")
                            {
                                prop.SetValue(materialTheme, Enum.Parse(propType, "Dark"));
                            }
                            else if (theme == "Light")
                            {
                                prop.SetValue(materialTheme, Enum.Parse(propType, "Light"));
                            }
                            else
                            {
                                prop.SetValue(materialTheme, Enum.Parse(propType, "Inherit"));
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        /// <summary>
        /// Шлях за замовчуванням для збереження створених відеопроєктів.
        /// При зміні перезавантажує список проєктів та зберігає налаштування.
        /// </summary>
        public string DefaultSavePath
        {
            get => _defaultSavePath;
            set 
            { 
                _defaultSavePath = value; 
                OnPropertyChanged(); 
                LoadProjects();
                SaveSettings();
            }
        }

        /// <summary>
        /// Зберігає поточні налаштування користувача (мову, тему, шлях до проєктів)
        /// у локальний файл конфігурації settings.json у форматі JSON.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                var settings = new
                {
                    SelectedLanguage = _selectedLanguage,
                    SelectedTheme = _selectedTheme,
                    DefaultSavePath = _defaultSavePath
                };
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Завантажує збережені налаштування користувача з файлу settings.json,
        /// якщо він існує, та ініціалізує відповідні поля конфігурації.
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("SelectedLanguage", out var langProp))
                    {
                        _selectedLanguage = langProp.GetString() ?? "English";
                    }
                    if (root.TryGetProperty("SelectedTheme", out var themeProp))
                    {
                        _selectedTheme = themeProp.GetString() ?? "System Default";
                    }
                    if (root.TryGetProperty("DefaultSavePath", out var pathProp))
                    {
                        _defaultSavePath = pathProp.GetString() ?? "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        public ObservableCollection<string> Languages { get; } = new() { "Czech", "English", "German", "Japanese", "Polish", "Slovak", "Ukrainian" };
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

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredProjects));
            }
        }

        private int _selectedFilterIndex = 0;
        public int SelectedFilterIndex
        {
            get => _selectedFilterIndex;
            set
            {
                _selectedFilterIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredProjects));
            }
        }

        private string _selectedSortOption = "DateDesc";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedSortText));
                OnPropertyChanged(nameof(FilteredProjects));
            }
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    CurrentPage = 1; // Reset to first page when size changes
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalPages));
                    OnPropertyChanged(nameof(FilteredProjects));
                }
            }
        }

        public ObservableCollection<int> PageSizes { get; } = new() { 20, 50, 100 };

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredProjects));
                }
            }
        }

        private int _totalItemsCount = 0;
        public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalItemsCount / PageSize));

        public void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        public void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        public void SetPageSize(string sizeStr)
        {
            if (int.TryParse(sizeStr, out int size))
            {
                PageSize = size;
            }
        }

        public string SelectedSortText
        {
            get
            {
                string key = _selectedSortOption switch
                {
                    "NameAsc" => "SortByNameAsc",
                    "NameDesc" => "SortByNameDesc",
                    "DateAsc" => "SortByDateAsc",
                    "DateDesc" or _ => "SortByDateDesc",
                };

                if (Avalonia.Application.Current != null && 
                    Avalonia.Application.Current.Resources.TryGetResource(key, null, out object? val) && 
                    val is string localizedString)
                {
                    return localizedString;
                }

                return _selectedSortOption switch
                {
                    "NameAsc" => "Name (A-Z)",
                    "NameDesc" => "Name (Z-A)",
                    "DateAsc" => "Oldest First",
                    "DateDesc" or _ => "Newest First",
                };
            }
        }

        public IEnumerable<VideoProject> FilteredProjects
        {
            get
            {
                IEnumerable<VideoProject> result = Projects;

                // Apply filtering based on selected tab
                if (_selectedFilterIndex == 1) // Recent: e.g. created in last 7 days
                {
                    var limit = DateTime.UtcNow.AddDays(-7);
                    result = result.Where(p => p.CreatedAt >= limit);
                }
                else if (_selectedFilterIndex == 2) // Favorites
                {
                    result = result.Where(p => p.IsFavorite);
                }

                // Apply search text filtering
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    result = result.Where(p => p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
                }

                // Apply sorting
                result = _selectedSortOption switch
                {
                    "NameAsc" => result.OrderBy(p => p.Name),
                    "NameDesc" => result.OrderByDescending(p => p.Name),
                    "DateAsc" => result.OrderBy(p => p.CreatedAt),
                    "DateDesc" or _ => result.OrderByDescending(p => p.CreatedAt),
                };

                int count = result.Count();
                if (_totalItemsCount != count)
                {
                    _totalItemsCount = count;
                    OnPropertyChanged(nameof(TotalPages));
                    if (_currentPage > TotalPages && TotalPages > 0)
                    {
                        _currentPage = TotalPages;
                        OnPropertyChanged(nameof(CurrentPage));
                    }
                }

                return result.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            }
        }

        public void SetSortOption(string option)
        {
            if (string.IsNullOrEmpty(option)) return;
            SelectedSortOption = option;
        }

        public void ToggleFavorite(VideoProject project)
        {
            if (project == null) return;
            project.IsFavorite = !project.IsFavorite;
            UpdateProject(project);
            OnPropertyChanged(nameof(FilteredProjects));
        }

        public ObservableCollection<VideoProject> Projects { get; set; } = new();
        public ObservableCollection<ConversionJob> ConversionJobs { get; set; } = new();

    /// <summary>
    /// Конструктор головної моделі подання MainWindowViewModel.
    /// Ініціалізує сервіси, завантажує збережені налаштування та застосовує мову та тему.
    /// </summary>
    public MainWindowViewModel()
    {
        // Вкажіть шлях до ffmpeg (для демонстрації припускаємо, що він в PATH або в тій же папці)
        _ffmpegService = new FFmpegService("ffmpeg");
        LoadSettings();
        LoadProjects();
        ApplyTheme(_selectedTheme);
        ApplyLanguage(_selectedLanguage);

        Projects.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredProjects));
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

        // Генеруємо мініатюру для відео
        try
        {
            var thumbnailFile = Path.Combine(projectDir, "thumbnail.jpg");
            if (File.Exists(project.SourceFilePath))
            {
                var thumbnailPath = await _ffmpegService.GenerateThumbnailAsync(project.SourceFilePath, thumbnailFile);
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    project.ThumbnailPath = thumbnailPath;
                }
            }
        }
        catch { }

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

        var formatExtension = string.IsNullOrWhiteSpace(project.Format) ? "mp4" : project.Format.TrimStart('.');
        var job = new ConversionJob
        {
            ProjectId = project.Id,
            PresetId = preset.Id,
            OutputPath = Path.Combine(exportPath, $"{project.Name}_{preset.Name}.{formatExtension}")
        };

        project.ExportFilePath = job.OutputPath;

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
            project.Progress = 100;
            project.StatusText = "Processing: 100.0%";
            // Keep IsProcessing true to show the progress bar
            
            // Fire and forget a 5 second delay to clear the UI
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    project.IsProcessing = false;
                    project.StatusText = "";
                });
            });
        }
        else
        {
            project.IsProcessing = false;
            project.StatusText = $"Error: {job.ErrorMessage}"; // Persists on failure
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

    public void UpdateProject(VideoProject project)
    {
        var safeProjectName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
        var projectDir = Path.Combine(DefaultSavePath, safeProjectName);
        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
        }

        try
        {
            var projectJson = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectDir, "project.json"), projectJson);
        }
        catch { }
        
        OnPropertyChanged(nameof(FilteredProjects));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
