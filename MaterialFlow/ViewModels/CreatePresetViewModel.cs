using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

public class CreatePresetViewModel : INotifyPropertyChanged
{
    private string _presetName = string.Empty;
    private Platform? _selectedPlatform;
    private string _resolution = "1920x1080";
    private string _bitrate = "5000";
    private string _codec = "libx264";
    private string _frameRate = "30";
    private string _errorMessage = string.Empty;
    private bool _isEditMode = false;
    private string _originalName = string.Empty;
    private bool _isManualMode = false;

    public string WindowTitle => _isEditMode
        ? $"{GetLocalizedString("TitleEditPreset")} {_originalName}"
        : GetLocalizedString("TitleCreatePreset");

    public ObservableCollection<Platform> Platforms { get; } = new();
    public ObservableCollection<string> Resolutions { get; } = new() { "1920x1080", "1280x720", "720x1280", "1080x1080", "2560x1440", "3840x2160", "1440x2560", "2160x3840" };
    public ObservableCollection<string> Bitrates { get; } = new() { "2000", "5000", "8000", "10000", "12000", "15000", "20000", "25000", "30000", "50000" };
    public ObservableCollection<string> FrameRates { get; } = new() { "24", "25", "30", "50", "60", "120" };
    public ObservableCollection<string> Codecs { get; } = new() { "libx264", "libx265", "mpeg4", "libvpx-vp9", "h264_nvenc", "hevc_nvenc" };

    public CreatePresetViewModel(Preset? presetToEdit = null)
    {
        foreach (var platform in DataService.Instance.Platforms)
        {
            Platforms.Add(platform);
        }

        if (Platforms.Count > 0)
        {
            SelectedPlatform = Platforms.First();
        }

        if (presetToEdit != null)
        {
            _isEditMode = true;
            _originalName = presetToEdit.Name;
            PresetName = presetToEdit.Name;
            SelectedPlatform = Platforms.FirstOrDefault(p => p.Id == presetToEdit.PlatformId) ?? SelectedPlatform;
            Resolution = presetToEdit.Resolution;
            Bitrate = presetToEdit.Bitrate.ToString();
            Codec = presetToEdit.Codec;
            FrameRate = presetToEdit.FrameRate.ToString();
        }
    }

    public string PresetName
    {
        get => _presetName;
        set { _presetName = value; OnPropertyChanged(); }
    }

    public Platform? SelectedPlatform
    {
        get => _selectedPlatform;
        set { _selectedPlatform = value; OnPropertyChanged(); }
    }

    public string Resolution
    {
        get => _resolution;
        set { _resolution = value; OnPropertyChanged(); }
    }

    public string Bitrate
    {
        get => _bitrate;
        set { _bitrate = value; OnPropertyChanged(); }
    }

    public string Codec
    {
        get => _codec;
        set { _codec = value; OnPropertyChanged(); }
    }

    public string FrameRate
    {
        get => _frameRate;
        set { _frameRate = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Вмикає/вимикає ручний режим введення налаштувань.
    /// </summary>
    public bool IsManualMode
    {
        get => _isManualMode;
        set { _isManualMode = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
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
    /// Перевіряє коректність заповнення полів нового пресета.
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(PresetName))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyPresetName");
            return false;
        }
        if (SelectedPlatform == null)
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyPlatform");
            return false;
        }
        if (!int.TryParse(Bitrate, out _) || int.Parse(Bitrate) <= 0)
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidBitrate");
            return false;
        }
        if (!int.TryParse(FrameRate, out _) || int.Parse(FrameRate) <= 0)
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidFPS");
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Повертає сформований об'єкт моделі Preset на основі заповнених полів форми.
    /// </summary>
    public Preset GetPresetData(Guid existingId)
    {
        return new Preset
        {
            Id = existingId == Guid.Empty ? Guid.NewGuid() : existingId,
            Name = PresetName,
            PlatformId = SelectedPlatform!.Id,
            Resolution = Resolution,
            Bitrate = int.Parse(Bitrate),
            Codec = Codec,
            FrameRate = int.Parse(FrameRate)
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}