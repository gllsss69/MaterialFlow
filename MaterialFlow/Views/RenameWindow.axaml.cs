using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;

namespace MaterialFlow.Views
{
    public partial class RenameWindow : Window
    {
        public string NewName => NameTextBox.Text?.Trim() ?? string.Empty;
        private readonly HashSet<string> _existingNames = new();
        private readonly string _currentName;

        public RenameWindow()
        {
            InitializeComponent();
            _currentName = string.Empty;
        }

        public RenameWindow(string currentName, IEnumerable<string> existingNames) : this()
        {
            _currentName = currentName;
            NameTextBox.Text = currentName;
            foreach (var name in existingNames)
            {
                _existingNames.Add(name);
            }
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void Save_Click(object? sender, RoutedEventArgs e)
        {
            ErrorTextBlock.IsVisible = false;

            if (string.IsNullOrWhiteSpace(NewName))
            {
                return;
            }

            if (NewName != _currentName && _existingNames.Contains(NewName))
            {
                ErrorTextBlock.IsVisible = true;
                return;
            }

            Close(NewName);
        }
    }
}
