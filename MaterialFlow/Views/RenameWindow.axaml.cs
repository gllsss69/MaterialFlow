using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MaterialFlow.Views
{
    public partial class RenameWindow : Window
    {
        public string NewName => NameTextBox.Text ?? string.Empty;

        public RenameWindow()
        {
            InitializeComponent();
        }

        public RenameWindow(string currentName) : this()
        {
            NameTextBox.Text = currentName;
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewName))
            {
                Close(NewName);
            }
        }
    }
}
