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

    public void SetPageIndex(string index)
    {
        if (int.TryParse(index, out int i)) CurrentPageIndex = i;
    }

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

    public ObservableCollection<string> Languages { get; } = new() { "Czech", "English", "German", "Japanese", "Polish", "Slovak", "Ukrainian" };
    public ObservableCollection<string> Themes { get; } = new() { "System Default", "Light", "Dark" };

    private void SaveSettings()
    {
        SettingsService.Instance.SelectedLanguage = _selectedLanguage;
        SettingsService.Instance.SelectedTheme = _selectedTheme;
        SettingsService.Instance.DefaultSavePath = _defaultSavePath;
        SettingsService.Instance.LastLoginUser = _lastLoginUser;
        SettingsService.Instance.EnableProjectLogging = _enableProjectLogging;
        SettingsService.Instance.Save();
    }

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
