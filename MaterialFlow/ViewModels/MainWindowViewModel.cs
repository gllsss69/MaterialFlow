using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Layout;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

/// <summary>
/// Головна ViewModel програми — координатор навігації, авторизації та дочірніх ViewModel-ів.
/// </summary>
public class MainWindowViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// ViewModel для списку проєктів (фільтри, сортування, пагінація, конвертація).
    /// </summary>
    public ProjectListViewModel ProjectList { get; }

    public MainWindowViewModel()
    {
        ProjectList = new ProjectListViewModel();

        SettingsService.Instance.Load();

        _selectedLanguage = SettingsService.Instance.SelectedLanguage;
        _selectedTheme = SettingsService.Instance.SelectedTheme;
        _defaultSavePath = SettingsService.Instance.DefaultSavePath;
        _enableProjectLogging = SettingsService.Instance.EnableProjectLogging;

        var lastLogin = SettingsService.Instance.LastLoginUser;
        if (!string.IsNullOrWhiteSpace(lastLogin))
        {
            _lastLoginUser = lastLogin;
            AuthService.Instance.RestoreSession(lastLogin);
        }

        ProjectList.LoadProjects(_defaultSavePath);
        ApplyTheme(_selectedTheme);
        ApplyLanguage(_selectedLanguage);
        CurrentUser = AuthService.Instance.CurrentUser;
    }


    private string _lastLoginUser = string.Empty;
    private User? _currentUser;
    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            _lastLoginUser = _currentUser?.Login ?? string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsLoggedOut));
            OnPropertyChanged(nameof(UserFullName));
            OnPropertyChanged(nameof(UserInitials));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsEditor));
            SaveSettings();
        }
    }

    public bool IsLoggedIn => CurrentUser != null;
    public bool IsLoggedOut => CurrentUser == null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsEditor => !IsAdmin;
    public string UserFullName => CurrentUser?.FullName ?? "Guest";
    public string UserInitials => string.IsNullOrWhiteSpace(CurrentUser?.FullName) ? "?" :
        new string(CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToArray()).ToUpper();

    /// <summary>
    /// Завершує сеанс поточного користувача та очищує авторизаційні дані.
    /// </summary>
    public void Logout()
    {
        AuthService.Instance.Logout();
        CurrentUser = null;
    }


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

    /// <summary>
    /// Встановлює індекс активної сторінки для навігації в головному вікні.
    /// </summary>
    /// <param name="index">Індекс цільової сторінки у вигляді рядка.</param>
    public void SetPageIndex(string index)
    {
        if (int.TryParse(index, out int i)) CurrentPageIndex = i;
    }

    /// <summary>
    /// Згортає або розгортає бічну навігаційну панель програми.
    /// </summary>
    public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

    private string _selectedLanguage = "English";
    private string _selectedTheme = "System Default";
    private string _defaultSavePath = "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";

    /// <summary>
    /// Поточна вибрана мова локалізації інтерфейсу програми.
    /// </summary>
    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            OnPropertyChanged();
            ApplyLanguage(value);
            ProjectList.RefreshLocalizations();
            SaveSettings();
        }
    }

    /// <summary>
    /// Поточна вибрана тема оформлення інтерфейсу.
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
    /// Шлях за замовчуванням для збереження відеопроєктів.
    /// </summary>
    public string DefaultSavePath
    {
        get => _defaultSavePath;
        set
        {
            _defaultSavePath = value;
            OnPropertyChanged();
            ProjectList.LoadProjects(_defaultSavePath);
            SaveSettings();
        }
    }

    private bool _enableProjectLogging;
    public bool EnableProjectLogging
    {
        get => _enableProjectLogging;
        set
        {
            _enableProjectLogging = value;
            OnPropertyChanged();
            SaveSettings();
        }
    }

    // --- FFmpeg Status ---
    private bool _isFFmpegAvailable;
    /// <summary>
    /// Прапорець доступності медіапроцесора FFmpeg у системі.
    /// </summary>
    public bool IsFFmpegAvailable
    {
        get => _isFFmpegAvailable;
        set { _isFFmpegAvailable = value; OnPropertyChanged(); OnPropertyChanged(nameof(FFmpegStatusText)); }
    }

    private string _ffmpegVersionText = "";
    /// <summary>
    /// Рядок версії FFmpeg, отриманий від медіапроцесора.
    /// </summary>
    public string FFmpegVersionText
    {
        get => _ffmpegVersionText;
        set { _ffmpegVersionText = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Текстове відображення статусу доступності FFmpeg.
    /// </summary>
    public string FFmpegStatusText => IsFFmpegAvailable ? "Available" : "Not Found";

    private bool _isCheckingFFmpeg;
    /// <summary>
    /// Прапорець, що вказує на виконання перевірки FFmpeg.
    /// </summary>
    public bool IsCheckingFFmpeg
    {
        get => _isCheckingFFmpeg;
        set { _isCheckingFFmpeg = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Асинхронно перевіряє доступність FFmpeg та отримує рядок його версії.
    /// </summary>
    public async System.Threading.Tasks.Task CheckFFmpegStatusAsync()
    {
        IsCheckingFFmpeg = true;
        try
        {
            var ffmpeg = new FFmpegService("ffmpeg");
            IsFFmpegAvailable = ffmpeg.IsFFmpegAvailable();

            if (IsFFmpegAvailable)
            {
                FFmpegVersionText = await ffmpeg.GetFFmpegVersionAsync();
            }
            else
            {
                FFmpegVersionText = "FFmpeg not found in system PATH";
            }
        }
        catch (Exception ex)
        {
            IsFFmpegAvailable = false;
            FFmpegVersionText = $"Error: {ex.Message}";
        }
        finally
        {
            IsCheckingFFmpeg = false;
        }
    }

    public ObservableCollection<string> Languages { get; } = new() { "Czech", "English", "German", "Japanese", "Polish", "Slovak", "Ukrainian" };
    public ObservableCollection<string> Themes { get; } = new() { "System Default", "Light", "Dark" };

    /// <summary>
    /// Зберігає поточні налаштування інтерфейсу користувача у файл конфігурації.
    /// </summary>
    private void SaveSettings()
    {
        SettingsService.Instance.SelectedLanguage = _selectedLanguage;
        SettingsService.Instance.SelectedTheme = _selectedTheme;
        SettingsService.Instance.DefaultSavePath = _defaultSavePath;
        SettingsService.Instance.LastLoginUser = _lastLoginUser;
        SettingsService.Instance.EnableProjectLogging = _enableProjectLogging;
        SettingsService.Instance.Save();
    }

    /// <summary>
    /// Завантажує та застосовує обрану мову локалізації інтерфейсу.
    /// </summary>
    /// <param name="languageName">Назва цільової мови.</param>
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

            if (existingDict != null) mergedDicts.Remove(existingDict);
            mergedDicts.Add(newDict);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load language {langCode}: {ex.Message}");
        }
    }

    /// <summary>
    /// Динамічно змінює та застосовує вибрану тему оформлення додатка.
    /// </summary>
    /// <param name="theme">Назва вибраної теми оформлення.</param>
    private void ApplyTheme(string theme)
    {
        if (Avalonia.Application.Current != null)
        {
            if (theme == "Dark")
                Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
            else if (theme == "Light")
                Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
            else
                Avalonia.Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Default;

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
                            prop.SetValue(materialTheme, Enum.Parse(propType, "Dark"));
                        else if (theme == "Light")
                            prop.SetValue(materialTheme, Enum.Parse(propType, "Light"));
                        else
                            prop.SetValue(materialTheme, Enum.Parse(propType, "Inherit"));
                    }
                    catch { }
                }
            }
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class FilterItem : INotifyPropertyChanged
{
    public string Name { get; }
    public string IconKind { get; }

    public FilterItem(string name, string iconKind)
    {
        Name = name;
        IconKind = iconKind;
    }

    public string DisplayName
    {
        get
        {
            string key = $"Filter{Name}";
            if (Avalonia.Application.Current != null)
            {
                var activeDict = Avalonia.Application.Current.Resources.MergedDictionaries
                    .OfType<Avalonia.Controls.ResourceDictionary>()
                    .LastOrDefault(d => d.ContainsKey("NavHome"));
                if (activeDict != null && activeDict.TryGetValue(key, out object? val) && val is string s)
                    return s;
            }
            return Name;
        }
    }

    public void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
