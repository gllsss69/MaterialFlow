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
    private int _bitrate = 5000;
    private string _codec = "libx264";
    private int _frameRate = 30;
    private string _errorMessage = string.Empty;
    private bool _isEditMode = false;
    private string _originalName = string.Empty;

    public string WindowTitle => _isEditMode ? $"Edit {_originalName}" : "Create Preset";

    public ObservableCollection<Platform> Platforms { get; } = new();
    public ObservableCollection<string> Resolutions { get; } = new() { "1920x1080", "1280x720", "720x1280", "1080x1080", "2560x1440", "3840x2160", "1440x2560", "2160x3840" };
    public ObservableCollection<int> Bitrates { get; } = new() { 2000, 5000, 8000, 10000, 12000, 15000, 20000, 25000, 30000, 50000 };
    public ObservableCollection<int> FrameRates { get; } = new() { 24, 25, 30, 50, 60, 120 };
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
            Bitrate = presetToEdit.Bitrate;
            Codec = presetToEdit.Codec;
            FrameRate = presetToEdit.FrameRate;
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

    public int Bitrate
    {
        get => _bitrate;
        set { _bitrate = value; OnPropertyChanged(); }
    }

    public string Codec
    {
        get => _codec;
        set { _codec = value; OnPropertyChanged(); }
    }

    public int FrameRate
    {
        get => _frameRate;
        set { _frameRate = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(PresetName))
        {
            ErrorMessage = "Preset name cannot be empty.";
            return false;
        }
        if (SelectedPlatform == null)
        {
            ErrorMessage = "Please select a platform.";
            return false;
        }
        
        ErrorMessage = string.Empty;
        return true;
    }

    public Preset GetPresetData(Guid existingId)
    {
        return new Preset
        {
            Id = existingId == Guid.Empty ? Guid.NewGuid() : existingId,
            Name = PresetName,
            PlatformId = SelectedPlatform!.Id,
            Resolution = Resolution,
            Bitrate = Bitrate,
            Codec = Codec,
            FrameRate = FrameRate
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
