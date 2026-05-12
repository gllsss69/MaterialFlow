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
    }
}