using Avalonia.Controls;
using Avalonia.Interactivity;
using MaterialFlow.Models;
using MaterialFlow.ViewModels;
using System.Threading.Tasks;

namespace MaterialFlow.Views;

public partial class PresetsView : UserControl
{
    private PresetsViewModel ViewModel => (PresetsViewModel)DataContext!;

    public PresetsView()
    {
        InitializeComponent();
        DataContext = new PresetsViewModel();
    }

    private async void CreatePreset_Click(object? sender, RoutedEventArgs e)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window == null) return;

        var dialog = new CreatePresetWindow();
        var result = await dialog.ShowDialog<bool>(window);

        if (result)
        {
            var newPreset = dialog.ViewModel.GetPresetData(System.Guid.Empty);
            Services.DataService.Instance.Presets.Add(newPreset);
            await Services.DataService.Instance.SavePresetsAsync();
            ViewModel.LoadData();
        }
    }

    private async void EditPreset_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Preset preset)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window == null) return;

            var dialog = new CreatePresetWindow(preset);
            var result = await dialog.ShowDialog<bool>(window);

            if (result)
            {
                var updatedPreset = dialog.ViewModel.GetPresetData(preset.Id);
                // Update in DataService
                var index = Services.DataService.Instance.Presets.FindIndex(p => p.Id == preset.Id);
                if (index >= 0)
                {
                    Services.DataService.Instance.Presets[index] = updatedPreset;
                    await Services.DataService.Instance.SavePresetsAsync();
                    ViewModel.LoadData();
                }
            }
        }
    }

    private async void DeletePreset_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Preset preset)
        {
            await ViewModel.DeletePresetAsync(preset);
        }
    }
}
