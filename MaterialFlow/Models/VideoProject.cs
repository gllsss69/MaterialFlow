using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaterialFlow.Models;

public class VideoProject : INotifyPropertyChanged
{
    private double _progress;
    private string _statusText = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string SourceFilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid UserId { get; set; }

    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private bool _isProcessing;
    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
