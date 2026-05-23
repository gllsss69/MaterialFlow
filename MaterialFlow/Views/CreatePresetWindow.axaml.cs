using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MaterialFlow.Models;
using MaterialFlow.ViewModels;

namespace MaterialFlow.Views;

public partial class CreatePresetWindow : Window
{
    private CreatePresetViewModel _viewModel;
    public CreatePresetViewModel ViewModel => _viewModel;

    public CreatePresetWindow()
    {
        InitializeComponent();
        DataContext = _viewModel = new CreatePresetViewModel();
    }

    public CreatePresetWindow(Preset presetToEdit)
    {
        InitializeComponent();
        DataContext = _viewModel = new CreatePresetViewModel(presetToEdit);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Validate())
        {
            Close(true);
        }
    }
}
