using Avalonia.Controls;
using MaterialFlow.ViewModels;

namespace MaterialFlow
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void CreateProject_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var dialog = new Views.CreateProjectWindow();
            await dialog.ShowDialog(this);
        }

        private void ToggleSidebar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleSidebar();
            }
        }
    }
}