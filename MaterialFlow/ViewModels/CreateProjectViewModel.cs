using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

namespace MaterialFlow.ViewModels;

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
    private string _selectedFormat = ".mp4";
    private bool _useWatermark = true;

    public string ProjectName
    {
        get => _projectName;
        set { _projectName = value; OnPropertyChanged(); }
    }

    public string SavePath
    {
        get => _savePath;
        set { _savePath = value; OnPropertyChanged(); }
    }

    public string SourceFilePath
    {
        get => _sourceFilePath;
        set { _sourceFilePath = value; OnPropertyChanged(); }
    }

    public string SourceFileName
    {
        get => _sourceFileName;
        set { _sourceFileName = value; OnPropertyChanged(); }
    }

    public bool IsSourceFileSelected
    {
        get => _isSourceFileSelected;
        set { _isSourceFileSelected = value; OnPropertyChanged(); }
    }

    public string SelectedPlatform
    {
        get => _selectedPlatform;
        set
        {
            if (_selectedPlatform != value)
            {
                _selectedPlatform = value;
                OnPropertyChanged();
                UpdateDefaultSettings();
            }
        }
    }

    public string SelectedResolution
    {
        get => _selectedResolution;
        set { _selectedResolution = value; OnPropertyChanged(); }
    }

    public string SelectedBitrate
    {
        get => _selectedBitrate;
        set { _selectedBitrate = value; OnPropertyChanged(); }
    }

    public string SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); }
    }

    public bool UseWatermark
    {
        get => _useWatermark;
        set { _useWatermark = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> Resolutions { get; } = new()
    {
        "1920x1080",
        "1280x720",
        "720x1280",
        "1080x1080",
        "3840x2160"
    };

    private void UpdateDefaultSettings()
    {
        switch (SelectedPlatform)
        {
            case "YouTube":
                SelectedResolution = "1920x1080";
                break;
            case "TikTok":
                SelectedResolution = "720x1280";
                break;
            case "Facebook":
            case "Instagram":
                SelectedResolution = "1080x1080";
                break;
            default:
                SelectedResolution = "1920x1080";
                break;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
