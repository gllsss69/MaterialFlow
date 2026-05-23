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
    private string _selectedPlatform = "YouTube";
    private string _selectedResolution = "1920x1080";
    private string _projectName = string.Empty;
    private string _savePath = string.Empty;
    private string _sourceFilePath = string.Empty;
    private string _sourceFileName = string.Empty;
    private bool _isSourceFileSelected = false;
    private string _selectedBitrate = "Auto";
    private string _selectedFPS = "30 fps";
    private string _selectedFormat = ".mp4";
    private string _selectedCodec = "Auto";
    private bool _useWatermark = true;
    private string _errorMessage = string.Empty;
    private Preset? _selectedPreset;

    public CreateProjectViewModel()
    {
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
                    SelectedBitrate = (_selectedPreset.Bitrate / 1000) + " Mbps";
                    SelectedFPS = _selectedPreset.FrameRate + " fps";
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

    /// <summary>
    /// Цільова платформа для публікації відео (YouTube, TikTok, Facebook тощо).
    /// Автоматично підбирає рекомендовані налаштування роздільної здатності.
    /// </summary>
    public string SelectedPlatform
    {
        get => _selectedPlatform;
        set
        {
            if (_selectedPlatform != value)
            {
                _selectedPlatform = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPresetSelectionVisible));
                UpdateDefaultSettings();
            }
        }
    }

    public bool IsPresetSelectionVisible => !string.Equals(SelectedPlatform, "None", System.StringComparison.OrdinalIgnoreCase);

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
        "3840x2160"
    };

    /// <summary>
    /// Колекція доступних варіантів бітрейту відео.
    /// </summary>
    public ObservableCollection<string> Bitrates { get; } = new()
    {
        "Auto",
        "2 Mbps",
        "5 Mbps",
        "8 Mbps",
        "10 Mbps",
        "15 Mbps",
        "20 Mbps",
        "30 Mbps",
        "50 Mbps"
    };

    /// <summary>
    /// Колекція доступних варіантів FPS.
    /// </summary>
    public ObservableCollection<string> FPSs { get; } = new()
    {
        "24 fps",
        "25 fps",
        "30 fps",
        "50 fps",
        "60 fps",
        "120 fps"
    };

    /// <summary>
    /// Колекція підтримуваних вихідних контейнерів/форматів відео.
    /// </summary>
    public ObservableCollection<string> Formats { get; } = new()
    {
        ".mp4",
        ".mkv",
        ".avi"
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
        "libvpx-vp9"
    };

    /// <summary>
    /// Оновлює рекомендовані параметри за замовчуванням при перемиканні платформи.
    /// </summary>
    private void UpdateDefaultSettings()
    {
        // Update presets for selected platform
        AvailablePresets.Clear();
        var platform = DataService.Instance.Platforms.FirstOrDefault(p => p.Name.Equals(SelectedPlatform, System.StringComparison.OrdinalIgnoreCase));
        if (platform != null)
        {
            foreach (var preset in DataService.Instance.Presets.Where(pr => pr.PlatformId == platform.Id))
            {
                AvailablePresets.Add(preset);
            }
        }
    }

    /// <summary>
    /// Перевіряє коректність заповнення форми створення проєкту.
    /// </summary>
    /// <returns>Значення true, якщо всі поля валідні; інакше false з описом у властивості ErrorMessage.</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ErrorMessage = "Project name cannot be empty.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SourceFilePath))
        {
            ErrorMessage = "Please select a source video file.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SavePath))
        {
            ErrorMessage = "Please select an export destination.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedPlatform))
        {
            ErrorMessage = "Please select a platform.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedResolution))
        {
            ErrorMessage = "Please select a resolution.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedBitrate))
        {
            ErrorMessage = "Please select a bitrate.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedFPS))
        {
            ErrorMessage = "Please select a frame rate (FPS).";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedFormat))
        {
            ErrorMessage = "Please select a video format.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(SelectedCodec))
        {
            ErrorMessage = "Please select a codec.";
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
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
