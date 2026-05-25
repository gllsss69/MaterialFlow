using Avalonia.Controls;
using Avalonia.Interactivity;
using MaterialFlow.Models;
using MaterialFlow.ViewModels;

namespace MaterialFlow.Views;

public partial class CreatePlatformWindow : Window
{
    private readonly CreatePlatformViewModel _viewModel;

    public CreatePlatformWindow()
    {
        InitializeComponent();
        _viewModel = new CreatePlatformViewModel(null);
        DataContext = _viewModel;
    }

    public CreatePlatformWindow(Platform platform)
    {
        InitializeComponent();
        _viewModel = new CreatePlatformViewModel(platform);
        DataContext = _viewModel;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        var platform = _viewModel.Save();
        if (platform != null)
        {
            Close(platform);
        }
    }
}
