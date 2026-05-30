using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

public class PresetsViewModel : INotifyPropertyChanged
{
    private string _searchText = string.Empty;

    public ObservableCollection<Preset> Presets { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FilteredPresets)); OnPropertyChanged(nameof(IsPresetsEmpty));
        }
    }

    public bool IsPresetsEmpty => !FilteredPresets.Any();
    public IEnumerable<Preset> FilteredPresets
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return Presets;
            }
            return Presets.Where(p => p.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) || 
                                    (DataService.Instance.Platforms.FirstOrDefault(pl => pl.Id == p.PlatformId)?.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }

    public PresetsViewModel()
    {
        LoadData();
    }

    /// <summary>
    /// Очищує та завантажує актуальний список пресетів із сховища.
    /// </summary>
    public void LoadData()
    {
        Presets.Clear();
        foreach (var preset in DataService.Instance.Presets)
        {
            Presets.Add(preset);
        }
        OnPropertyChanged(nameof(FilteredPresets)); OnPropertyChanged(nameof(IsPresetsEmpty));
    }

    /// <summary>
    /// Допоміжний метод для отримання назви платформи за її унікальним ідентифікатором.
    /// </summary>
    public string GetPlatformName(Guid platformId)
    {
        return DataService.Instance.Platforms.FirstOrDefault(p => p.Id == platformId)?.Name ?? "Unknown";
    }

    /// <summary>
    /// Асинхронно видаляє вибраний пресет та оновлює конфігураційний файл presets.json.
    /// </summary>
    public async Task DeletePresetAsync(Preset preset)
    {
        if (preset != null)
        {
            DataService.Instance.Presets.Remove(preset);
            Presets.Remove(preset);
            await DataService.Instance.SavePresetsAsync();
            OnPropertyChanged(nameof(FilteredPresets)); OnPropertyChanged(nameof(IsPresetsEmpty));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
