using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

/// <summary>
/// Модель подання для створення нового відеопроєкту.
/// Керує параметрами конфігурації проєкту, вибором джерельного файлу та валідацією полів.
/// </summary>
public class CreateProjectViewModel : INotifyPropertyChanged
{
    private string _selectedResolution = "1920x1080";
    private string _projectName = string.Empty;
    private string _savePath = string.Empty;
    private string _sourceFilePath = string.Empty;
    private string _sourceFileName = string.Empty;
    private bool _isSourceFileSelected = false;
    private string _selectedBitrate = "Auto";
    private string _selectedFPS = "30";
    private string _selectedFormat = ".mp4";
    private string _selectedCodec = "Auto";
    private bool _useWatermark = true;
    private string _errorMessage = string.Empty;
    private Preset? _selectedPreset;
    private bool _isManualMode = false;

    private ObservableCollection<Platform> _availablePlatforms = new();

    public CreateProjectViewModel()
    {
        // build a stable collection so bindings (SelectedItem) work reliably
        _availablePlatforms = new ObservableCollection<Platform>(DataService.Instance.Platforms);
        _availablePlatforms.Add(new Platform { Name = "None", IconKind = "Cancel" });

        _selectedPlatform = _availablePlatforms.FirstOrDefault();
        UpdateDefaultSettings();
    }

    public ObservableCollection<Preset> AvailablePresets { get; } = new();

    public Preset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (_selectedPreset != value)
            {
                _selectedPreset = value;
                OnPropertyChanged();
                
                if (_selectedPreset != null)
                {
                    SelectedResolution = _selectedPreset.Resolution;
                    
                    var bitrateStr = $"{_selectedPreset.Bitrate}";
                    if (Bitrates.Contains(bitrateStr))
                        SelectedBitrate = bitrateStr;
                    else
                        SelectedBitrate = "Auto";
                        
                    var fpsStr = $"{_selectedPreset.FrameRate}";
                    if (FPSs.Contains(fpsStr))
                        SelectedFPS = fpsStr;
                        
                    if (Codecs.Contains(_selectedPreset.Codec))
                        SelectedCodec = _selectedPreset.Codec;
                }
            }
        }
    }

    /// <summary>
    /// Текст повідомлення про помилку валідації полів форми.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Назва створюваного проєкту.
    /// </summary>
    public string ProjectName
    {
        get => _projectName;
        set { _projectName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Шлях збереження (експорту) майбутнього проєкту.
    /// </summary>
    public string SavePath
    {
        get => _savePath;
        set { _savePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Повний файловий шлях до джерельного відеофайлу.
    /// </summary>
    public string SourceFilePath
    {
        get => _sourceFilePath;
        set { _sourceFilePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Ім'я файлу джерельного відео (без повної адреси папки).
    /// </summary>
    public string SourceFileName
    {
        get => _sourceFileName;
        set { _sourceFileName = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Вказує, чи було успішно вибрано джерельний відеофайл.
    /// </summary>
    public bool IsSourceFileSelected
    {
        get => _isSourceFileSelected;
        set { _isSourceFileSelected = value; OnPropertyChanged(); }
    }

    private Platform? _selectedPlatform;

    /// <summary>
    /// Цільова платформа для публікації відео.
    /// </summary>
    public IEnumerable<Platform> AvailablePlatforms => _availablePlatforms;

    /// <summary>
    /// Назва цільової платформи. "None" означає ручні налаштування.
    /// </summary>
    public Platform? SelectedPlatform
    {
        get => _selectedPlatform;
        set
        {
            if (_selectedPlatform != value)
            {
                _selectedPlatform = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPresetSelectionVisible));
                OnPropertyChanged(nameof(IsManualSettingsEnabled));
                UpdateDefaultSettings();
            }
        }
    }

    public bool IsPresetSelectionVisible => SelectedPlatform != null && !string.Equals(SelectedPlatform.Name, "None", System.StringComparison.OrdinalIgnoreCase);
    public bool IsManualSettingsEnabled => _isManualMode || SelectedPlatform == null || string.Equals(SelectedPlatform.Name, "None", System.StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Вмикає/вимикає ручний режим редагування налаштувань відео.
    /// </summary>
    public bool IsManualMode
    {
        get => _isManualMode;
        set { _isManualMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsManualSettingsEnabled)); }
    }

    /// <summary>
    /// Очікувана роздільна здатність вихідного файлу.
    /// </summary>
    public string SelectedResolution
    {
        get => _selectedResolution;
        set { _selectedResolution = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Очікуваний бітрейт вихідного відео файлу.
    /// </summary>
    public string SelectedBitrate
    {
        get => _selectedBitrate;
        set { _selectedBitrate = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Очікувана частота кадрів вихідного відео файлу.
    /// </summary>
    public string SelectedFPS
    {
        get => _selectedFPS;
        set { _selectedFPS = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Формат медіафайлу (наприклад, .mp4, .mkv).
    /// </summary>
    public string SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Очікуваний відеокодек.
    /// </summary>
    public string SelectedCodec
    {
        get => _selectedCodec;
        set { _selectedCodec = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Вказує, чи потрібно накладати водяний знак під час рендерингу.
    /// </summary>
    public bool UseWatermark
    {
        get => _useWatermark;
        set { _useWatermark = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Колекція доступних варіантів роздільної здатності відео.
    /// </summary>
    public ObservableCollection<string> Resolutions { get; } = new()
    {
        "1920x1080",
        "1280x720",
        "720x1280",
        "1080x1080",
        "2560x1440",
        "3840x2160",
        "1440x2560",
        "2160x3840"
    };

    /// <summary>
    /// Колекція доступних варіантів бітрейту відео.
    /// </summary>
    public ObservableCollection<string> Bitrates { get; } = new()
    {
        "Auto",
        "2000",
        "5000",
        "8000",
        "10000",
        "12000",
        "15000",
        "20000",
        "25000",
        "30000",
        "50000"
    };

    /// <summary>
    /// Колекція доступних варіантів FPS.
    /// </summary>
    public ObservableCollection<string> FPSs { get; } = new()
    {
        "24",
        "25",
        "30",
        "50",
        "60",
        "120"
    };

    /// <summary>
    /// Колекція підтримуваних вихідних контейнерів/форматів відео.
    /// </summary>
    public ObservableCollection<string> Formats { get; } = new()
    {
        ".mp4",
        ".mkv",
        ".avi",
        ".mov",
        ".webm"
    };

    /// <summary>
    /// Колекція доступних варіантів кодеків відео.
    /// </summary>
    public ObservableCollection<string> Codecs { get; } = new()
    {
        "Auto",
        "libx264",
        "libx265",
        "mpeg4",
        "libvpx-vp9",
        "h264_nvenc",
        "hevc_nvenc"
    };

    /// <summary>
    /// Оновлює рекомендовані параметри за замовчуванням при перемиканні платформи.
    /// </summary>
    private void UpdateDefaultSettings()
    {
        // Update presets for selected platform
        AvailablePresets.Clear();
        if (SelectedPlatform != null && !string.Equals(SelectedPlatform.Name, "None", System.StringComparison.OrdinalIgnoreCase))
        {
            // Set values from platform defaults
            SelectedResolution = SelectedPlatform.DefaultResolution;
            
            var bitrateKbps = SelectedPlatform.DefaultBitrate;
            var bitrateStr = $"{bitrateKbps}";
            if (Bitrates.Contains(bitrateStr))
                SelectedBitrate = bitrateStr;
            else
                SelectedBitrate = "Auto";
                
            var fpsStr = $"{SelectedPlatform.DefaultFPS} fps";
            if (FPSs.Contains(fpsStr))
                SelectedFPS = fpsStr;
                
            if (Codecs.Contains(SelectedPlatform.DefaultCodec))
                SelectedCodec = SelectedPlatform.DefaultCodec;

            // Load available presets for this platform
            foreach (var preset in DataService.Instance.Presets.Where(pr => pr.PlatformId == SelectedPlatform.Id))
            {
                AvailablePresets.Add(preset);
            }
        }
        
        // Don't auto-select preset
        SelectedPreset = null;
    }

    private string GetLocalizedString(string key)
    {
        if (Avalonia.Application.Current?.Resources.TryGetResource(key, null, out var value) == true && value is string s)
        {
            return s;
        }
        return key;
    }

    /// <summary>
    /// Перевіряє коректність заповнення форми створення проєкту.
    /// </summary>
    /// <returns>Значення true, якщо всі поля валідні; інакше false з описом у властивості ErrorMessage.</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyProjectName");
            return false;
        }
        if (string.IsNullOrWhiteSpace(SourceFilePath))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptySourceFile");
            return false;
        }
        if (string.IsNullOrWhiteSpace(SavePath))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptySavePath");
            return false;
        }
        if (SelectedPlatform == null)
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyPlatform");
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedResolution))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyResolution");
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(SelectedResolution.Trim(), @"^\d+x\d+$"))
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidResolution");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedBitrate))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyBitrate");
            return false;
        }
        if (!string.Equals(SelectedBitrate, "Auto", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(SelectedBitrate, out int bitrateVal) || bitrateVal <= 0)
            {
                ErrorMessage = GetLocalizedString("ErrorInvalidBitrate");
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(SelectedFPS))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyFPS");
            return false;
        }
        if (!int.TryParse(SelectedFPS, out int fpsVal) || fpsVal <= 0)
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidFPS");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedFormat))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyFormat");
            return false;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(SelectedFormat.Trim(), @"^\.?\w+$"))
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidFormat");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedCodec))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyCodec");
            return false;
        }
        if (!string.Equals(SelectedCodec, "Auto", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(SelectedCodec.Trim(), @"^[\w\-]+$"))
            {
                ErrorMessage = GetLocalizedString("ErrorInvalidCodec");
                return false;
            }
        }

        ErrorMessage = string.Empty;
        return true;
    }

    private bool _enableProjectLogging;
    public bool EnableProjectLogging
    {
        get => _enableProjectLogging;
        set { _enableProjectLogging = value; OnPropertyChanged(); }
    }

    // Call this after project creation
    /// <summary>
    /// Створює текстовий файл журналу (log) з метаданими проєкту, якщо увімкнено логування.
    /// </summary>
    public void CreateLogFileIfEnabled()
    {
        if (!EnableProjectLogging || string.IsNullOrWhiteSpace(SavePath)) return;
        try
        {
            var logPath = System.IO.Path.Combine(SavePath, "project_log.txt");
            var logContent = $"Project: {ProjectName}\nCreated: {System.DateTime.Now}\nSource: {SourceFilePath}\n";
            System.IO.File.WriteAllText(logPath, logContent);
        }
        catch
        {
            // Optionally handle/log error
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Викликає подію зміни значення властивості для оновлення елементів інтерфейсу.
    /// </summary>
    /// <param name="propertyName">Ім'я властивості, що змінилась.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
