using Avalonia.Controls;
using Avalonia.Interactivity;
using MaterialFlow.Models;
using MaterialFlow.ViewModels;
using System.Threading.Tasks;

namespace MaterialFlow.Views;

public partial class PlatformsView : UserControl
{
    public PlatformsView()
    {
        InitializeComponent();
    }

    private async void CreatePlatform_Click(object? sender, RoutedEventArgs e)
    {
        var window = new CreatePlatformWindow();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window parentWindow)
        {
            var newPlatform = await window.ShowDialog<Platform?>(parentWindow);
            if (newPlatform != null)
            {
                if (DataContext is PlatformsViewModel vm)
                {
                    vm.LoadData();
                }
            }
        }
    }

    private async void EditPlatform_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Platform platform)
        {
            var window = new CreatePlatformWindow(platform);
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window parentWindow)
            {
                var editedPlatform = await window.ShowDialog<Platform?>(parentWindow);
                if (editedPlatform != null)
                {
                    if (DataContext is PlatformsViewModel vm)
                    {
                        vm.LoadData();
                    }
                }
            }
        }
    }

    private async void DeletePlatform_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Platform platform)
        {

            if (DataContext is PlatformsViewModel vm)
            {
                await vm.DeletePlatformAsync(platform);
            }
        }
    }
}
