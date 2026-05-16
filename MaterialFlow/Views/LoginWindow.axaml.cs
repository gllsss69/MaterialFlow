using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MaterialFlow.ViewModels;

namespace MaterialFlow.Views;

public partial class LoginWindow : Window
{
    private LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new LoginViewModel();
    }

    private void Auth_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Authenticate())
        {
            Close(true);
        }
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void SwitchMode_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.IsRegisterMode = !_viewModel.IsRegisterMode;
        _viewModel.Message = string.Empty;
    }
}
