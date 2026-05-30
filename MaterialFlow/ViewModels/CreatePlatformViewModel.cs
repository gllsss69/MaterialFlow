using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

public record PlatformIconItem(string Kind, string Label);

public class CreatePlatformViewModel : INotifyPropertyChanged
{
    private Platform? _editingPlatform;
    private string _platformName = string.Empty;
    private PlatformIconItem _iconKind;
    private string _errorMessage = string.Empty;
    private bool _isManualMode = false;
    
    private string _resolution = "1920x1080";
    private string _bitrate = "5000";
    private string _frameRate = "30";
    private string _codec = "libx264";

    public string WindowTitle => _editingPlatform == null ? "Create Platform" : "Edit Platform";
    public string SubmitButtonText => _editingPlatform == null ? "Create" : "Save";

    public string PlatformName
    {
        get => _platformName;
        set { _platformName = value; OnPropertyChanged(); }
    }

    public PlatformIconItem IconKind
    {
        get => _iconKind;
        set { _iconKind = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
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

    public string Codec
    {
        get => _codec;
        set { _codec = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> Resolutions { get; } = new() { "1920x1080", "1280x720", "720x1280", "1080x1080", "2560x1440", "3840x2160", "1440x2560", "2160x3840" };
    public ObservableCollection<string> Bitrates { get; } = new() { "2000", "5000", "8000", "10000", "12000", "15000", "20000", "25000", "30000", "50000" };
    public ObservableCollection<string> FrameRates { get; } = new() { "24", "25", "30", "50", "60", "120" };
    public ObservableCollection<string> Codecs { get; } = new() { "libx264", "libx265", "mpeg4", "libvpx-vp9", "h264_nvenc", "hevc_nvenc" };

    public ObservableCollection<PlatformIconItem> Icons { get; } = new()
    {
        new("Youtube",    "YouTube"),
        new("MusicNote",  "TikTok"),
        new("Instagram",  "Instagram"),
        new("Facebook",   "Facebook"),
        new("Twitter",    "Twitter"),
        new("Twitch",     "Twitch"),
        new("Monitor",    "Desktop"),
        new("Video",      "Video"),
        new("Web",        "Web / Other"),
    };

    public CreatePlatformViewModel(Platform? platform = null)
    {
        _editingPlatform = platform;
        _iconKind = Icons[0];          // default

        if (platform != null)
        {
            PlatformName = platform.Name;
            IconKind = Icons.FirstOrDefault(i => i.Kind == platform.IconKind) ?? Icons[0];
            Resolution = platform.DefaultResolution;
            Bitrate = platform.DefaultBitrate.ToString();
            FrameRate = platform.DefaultFPS.ToString();
            Codec = platform.DefaultCodec;
        }
    }

    /// <summary>
    /// Валідує введені дані платформи, додає її в DataService та зберігає зміни у файл.
    /// </summary>
    /// <returns>Об'єкт створеної чи оновленої моделі Platform у разі успіху; null — у разі помилки.</returns>
        private string GetLocalizedString(string key)
    {
        if (Avalonia.Application.Current?.Resources.TryGetResource(key, null, out var value) == true && value is string s)
        {
            return s;
        }
        return key;
    }

    public Platform? Save()
    {
        if (string.IsNullOrWhiteSpace(PlatformName))
        {
            ErrorMessage = GetLocalizedString("ErrorEmptyPlatformName");
            return null;
        }

        if (_editingPlatform == null &&
            DataService.Instance.Platforms.Any(p => p.Name.Equals(PlatformName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ErrorMessage = GetLocalizedString("ErrorPlatformExists");
            return null;
        }

        if (!int.TryParse(Bitrate, out int bitrateValue) || bitrateValue <= 0)
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidBitrate");
            return null;
        }
        if (!int.TryParse(FrameRate, out int fpsValue) || fpsValue <= 0)
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidFPS");
            return null;
        }

                if (!System.Text.RegularExpressions.Regex.IsMatch(Resolution?.Trim() ?? "", @"^\d+x\d+$"))
        {
            ErrorMessage = GetLocalizedString("ErrorInvalidResolution");
            return null;
        }

        var platform = _editingPlatform ?? new Platform();
        platform.Name = PlatformName.Trim();
        platform.IconKind = IconKind.Kind;
        platform.DefaultResolution = Resolution;
        platform.DefaultBitrate = bitrateValue;
        platform.DefaultFPS = fpsValue;
        platform.DefaultCodec = Codec;

        if (_editingPlatform == null)
            DataService.Instance.Platforms.Add(platform);

        _ = DataService.Instance.SavePlatformsAsync();
        return platform;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
