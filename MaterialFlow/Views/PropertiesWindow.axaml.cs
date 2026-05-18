using Avalonia.Controls;
using Avalonia.Interactivity;
using MaterialFlow.Models;
using System.IO;

namespace MaterialFlow.Views
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow()
        {
            InitializeComponent();
        }

        public PropertiesWindow(VideoProject project) : this()
        {
            DataContext = new PropertiesViewModel(project);
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class PropertiesViewModel
    {
        private readonly VideoProject _project;
        public string Name => _project.Name;
        public string Resolution => !string.IsNullOrEmpty(_project.Resolution) ? _project.Resolution : "Unknown";
        public string Bitrate => !string.IsNullOrEmpty(_project.Bitrate) ? _project.Bitrate : "Unknown";
        public string Format => !string.IsNullOrEmpty(_project.Format) ? _project.Format : (!string.IsNullOrEmpty(_project.ExportFilePath) || !string.IsNullOrEmpty(_project.SourceFilePath) ? System.IO.Path.GetExtension(!string.IsNullOrEmpty(_project.ExportFilePath) ? _project.ExportFilePath : _project.SourceFilePath)?.TrimStart('.') ?? "Unknown" : "Unknown");
        public string FPS => !string.IsNullOrEmpty(_project.FPS) ? _project.FPS : "Unknown";
        public string Size
        {
            get
            {
                // Prioritize the exported file size if it exists
                string filePathToCheck = !string.IsNullOrEmpty(_project.ExportFilePath) ? _project.ExportFilePath : _project.SourceFilePath;
                if (!string.IsNullOrEmpty(filePathToCheck) && System.IO.File.Exists(filePathToCheck))
                {
                    try
                    {
                        var fileInfo = new System.IO.FileInfo(filePathToCheck);
                        double sizeInMb = fileInfo.Length / (1024.0 * 1024.0);
                        return $"{sizeInMb:F2} MB";
                    }
                    catch
                    {
                        return "Unknown";
                    }
                }
                return "Unknown (File not found)";
            }
        }
        public string Path => !string.IsNullOrEmpty(_project.ExportFilePath) ? _project.ExportFilePath : _project.SourceFilePath;

        public PropertiesViewModel(VideoProject project)
        {
            _project = project;
        }
    }
}
