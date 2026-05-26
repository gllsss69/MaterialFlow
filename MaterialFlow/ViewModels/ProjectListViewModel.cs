using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

/// <summary>
/// ViewModel для керування списком проєктів, фільтрацією, сортуванням, пагінацією та конвертацією.
/// </summary>
public class ProjectListViewModel : INotifyPropertyChanged
{
    private readonly FFmpegService _ffmpegService;
    private readonly Dictionary<Guid, CancellationTokenSource> _cancellationTokens = new();

    public ProjectListViewModel()
    {
        _ffmpegService = new FFmpegService("ffmpeg");
        _selectedStatusFilter = StatusFilters[0];
        _selectedPlatformFilter = PlatformFilters.First();
        Projects.CollectionChanged += (s, e) => UpdateFilteredProjects();
    }

    public ObservableCollection<VideoProject> Projects { get; set; } = new();
    public ObservableCollection<ConversionJob> ConversionJobs { get; set; } = new();


    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            UpdateFilteredProjects();
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
            OnPropertyChanged(nameof(EmptyStateIconKind));
            UpdateFilteredProjects();
        }
    }

    public string EmptyStateIconKind => _selectedFilterIndex == 2 ? "StarOutline" : "FolderOpenOutline";

    public ObservableCollection<FilterItem> StatusFilters { get; } = new()
    {
        new("All", "FilterVariant"),
        new("Completed", "CheckCircle"),
        new("Error", "AlertCircle"),
        new("Processing", "ProgressClock")
    };

    public IEnumerable<FilterItem> PlatformFilters
    {
        get
        {
            var list = new List<FilterItem> { new("All", "FilterVariant") };
            list.AddRange(DataService.Instance.Platforms.Select(p => new FilterItem(p.Name, p.IconKind)));
            return list;
        }
    }

    private FilterItem _selectedStatusFilter;
    public FilterItem SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            _selectedStatusFilter = value;
            OnPropertyChanged();
            UpdateFilteredProjects();
        }
    }

    private FilterItem _selectedPlatformFilter;
    public FilterItem SelectedPlatformFilter
    {
        get => _selectedPlatformFilter;
        set
        {
            _selectedPlatformFilter = value;
            OnPropertyChanged();
            UpdateFilteredProjects();
        }
    }

    public void SetStatusFilterCommand(FilterItem item) => SelectedStatusFilter = item;
    public void SetPlatformFilterCommand(FilterItem item) => SelectedPlatformFilter = item;

    private string _selectedSortOption = "DateDesc";
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            _selectedSortOption = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedSortText));
            UpdateFilteredProjects();
        }
    }

    public void SetSortOption(string option)
    {
        if (string.IsNullOrEmpty(option)) return;
        SelectedSortOption = option;
    }

    public void RefreshLocalizations()
    {
        foreach (var item in StatusFilters) item.Refresh();
        foreach (var item in PlatformFilters) item.Refresh();
        
        OnPropertyChanged(nameof(SelectedStatusFilter));
        OnPropertyChanged(nameof(SelectedPlatformFilter));
        OnPropertyChanged(nameof(SelectedSortText));
        OnPropertyChanged(nameof(StatusFilters));
        OnPropertyChanged(nameof(PlatformFilters));
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

            if (Avalonia.Application.Current != null)
            {
                var activeDict = Avalonia.Application.Current.Resources.MergedDictionaries
                    .OfType<Avalonia.Controls.ResourceDictionary>()
                    .LastOrDefault(d => d.ContainsKey("NavHome"));
                if (activeDict != null && activeDict.TryGetValue(key, out object? val) && val is string s)
                    return s;
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


    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (_pageSize != value)
            {
                _pageSize = value;
                CurrentPage = 1;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPages));
                UpdateFilteredProjects();
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
                UpdateFilteredProjects();
            }
        }
    }

    private int _totalItemsCount = 0;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)_totalItemsCount / PageSize));
    public bool IsProjectsEmpty => _totalItemsCount == 0;

    public void NextPage()
    {
        if (CurrentPage < TotalPages) CurrentPage++;
    }

    public void PreviousPage()
    {
        if (CurrentPage > 1) CurrentPage--;
    }

    public void SetPageSize(string sizeStr)
    {
        if (int.TryParse(sizeStr, out int size)) PageSize = size;
    }


    private IEnumerable<VideoProject> _filteredProjects = Array.Empty<VideoProject>();
    public IEnumerable<VideoProject> FilteredProjects => _filteredProjects;

    private void UpdateFilteredProjects()
    {
        IEnumerable<VideoProject> result = Projects;

        if (_selectedFilterIndex == 1) // Recent
        {
            var limit = DateTime.UtcNow.AddDays(-7);
            result = result.Where(p => p.CreatedAt >= limit);
        }
        else if (_selectedFilterIndex == 2) // Favorites
        {
            result = result.Where(p => p.IsFavorite);
        }

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            result = result.Where(p => p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }

        if (_selectedPlatformFilter != null && _selectedPlatformFilter.Name != "All")
        {
            result = result.Where(p => p.Platform != null && p.Platform.Equals(_selectedPlatformFilter.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (_selectedStatusFilter != null && _selectedStatusFilter.Name != "All")
        {
            result = _selectedStatusFilter.Name switch
            {
                "Completed" => result.Where(p => p.Progress >= 100 && (string.IsNullOrEmpty(p.StatusText) || p.StatusText == "Completed")),
                "Error" => result.Where(p => !string.IsNullOrEmpty(p.StatusText) && p.StatusText.StartsWith("Error")),
                "Processing" => result.Where(p => p.IsProcessing),
                _ => result
            };
        }

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
            OnPropertyChanged(nameof(IsProjectsEmpty));
        }
        if (_currentPage > TotalPages && TotalPages > 0)
        {
            _currentPage = TotalPages;
            OnPropertyChanged(nameof(CurrentPage));
        }

        _filteredProjects = result.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
        OnPropertyChanged(nameof(FilteredProjects));
    }


    public void ToggleFavorite(VideoProject project)
    {
        if (project == null) return;
        project.IsFavorite = !project.IsFavorite;
        UpdateProject(project);
        UpdateFilteredProjects();
    }

    public void LoadProjects(string defaultSavePath)
    {
        Projects.Clear();
        if (!Directory.Exists(defaultSavePath)) return;

        try
        {
            var directories = Directory.GetDirectories(defaultSavePath);
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
                        project.StatusText = project.Progress >= 100 ? "" : "Error";
                        Projects.Add(project);
                    }
                }
            }
        }
        catch { }
    }

    public void UpdateProject(VideoProject project)
    {
        var defaultSavePath = SettingsService.Instance.DefaultSavePath;
        var safeProjectName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
        var projectDir = Path.Combine(defaultSavePath, safeProjectName);
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

        UpdateFilteredProjects();
    }

    public async Task StartConversionAsync(VideoProject project, Preset preset, string exportPath)
    {
        if (project == null || preset == null) return;

        var defaultSavePath = SettingsService.Instance.DefaultSavePath;
        var safeProjectName = string.Join("_", project.Name.Split(Path.GetInvalidFileNameChars()));
        var projectDir = Path.Combine(defaultSavePath, safeProjectName);

        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
        }

        try
        {
            var thumbnailFile = Path.Combine(projectDir, "thumbnail.jpg");
            if (File.Exists(project.SourceFilePath))
            {
                var thumbnailPath = await _ffmpegService.GenerateThumbnailAsync(project.SourceFilePath, thumbnailFile);
                if (!string.IsNullOrEmpty(thumbnailPath))
                {
                    project.ThumbnailPath = string.Empty;
                    project.ThumbnailPath = thumbnailPath;
                }
            }
        }
        catch { }

        try
        {
            var projectJson = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectDir, "project.json"), projectJson);
        }
        catch { }

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

        var exportDir = Path.GetDirectoryName(job.OutputPath);
        if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        ConversionJobs.Add(job);

        project.IsProcessing = true;
        project.StatusText = "Pending in queue...";
        project.Progress = 0;

        var progress = new Progress<double>(p =>
        {
            job.Progress = p;
            project.Progress = p;
            project.StatusText = p == 0 ? "Processing..." : $"Processing: {p:F1}%";
        });

        string? logFilePath = SettingsService.Instance.EnableProjectLogging
            ? Path.Combine(projectDir, "conversion.log")
            : null;

        var cts = new CancellationTokenSource();
        _cancellationTokens[project.Id] = cts;

        bool success = await _ffmpegService.ConvertVideoAsync(project, preset, job, progress, cts.Token, logFilePath);

        _cancellationTokens.Remove(project.Id);

        if (success)
        {
            project.Progress = 100;
            project.StatusText = "Processing: 100.0%";

            try
            {
                var fileInfo = new FileInfo(job.OutputPath);
                var outputFile = new OutputFile
                {
                    JobId = job.Id,
                    FilePath = job.OutputPath,
                    Size = fileInfo.Exists ? fileInfo.Length : 0,
                    CreatedAt = DateTime.UtcNow
                };

                DataService.Instance.OutputFiles.Add(outputFile);
                _ = DataService.Instance.SaveOutputFilesAsync();
            }
            catch { }

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
            project.StatusText = $"Error: {job.ErrorMessage}";
        }

        try
        {
            var projectJson = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(projectDir, "project.json"), projectJson);
        }
        catch { }

        OnPropertyChanged(nameof(ConversionJobs));
    }

    public void CancelConversion(Guid projectId)
    {
        if (_cancellationTokens.TryGetValue(projectId, out var cts))
        {
            cts.Cancel();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
