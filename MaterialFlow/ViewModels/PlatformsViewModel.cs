using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

public class PlatformsViewModel : INotifyPropertyChanged
{
    private string _searchText = string.Empty;

    public ObservableCollection<Platform> Platforms { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilteredPlatforms)); OnPropertyChanged(nameof(IsPlatformsEmpty));
        }
    }

    public bool IsPlatformsEmpty => !FilteredPlatforms.Any();

    public System.Collections.Generic.IEnumerable<Platform> FilteredPlatforms
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return Platforms;
            }
            return Platforms.Where(p => p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }
    }

    public PlatformsViewModel()
    {
        LoadData();
    }

    /// <summary>
    /// Очищує та завантажує актуальний список платформ із бази даних DataService.
    /// </summary>
    public void LoadData()
    {
        Platforms.Clear();
        foreach (var platform in DataService.Instance.Platforms)
        {
            Platforms.Add(platform);
        }
        OnPropertyChanged(nameof(FilteredPlatforms)); OnPropertyChanged(nameof(IsPlatformsEmpty));
    }

    /// <summary>
    /// Асинхронно видаляє вибрану платформу та зберігає зміни на диску.
    /// </summary>
    public async Task DeletePlatformAsync(Platform platform)
    {
        if (platform != null)
        {
            DataService.Instance.Platforms.Remove(platform);
            Platforms.Remove(platform);
            await DataService.Instance.SavePlatformsAsync();
            OnPropertyChanged(nameof(FilteredPlatforms)); OnPropertyChanged(nameof(IsPlatformsEmpty));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
