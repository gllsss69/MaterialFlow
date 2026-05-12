using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MaterialFlow.Models;

namespace MaterialFlow.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public ObservableCollection<VideoProject> Projects { get; set; } = new();

    public MainWindowViewModel()
    {
        // Adding mock data for design verification
        Projects.Add(new VideoProject 
        { 
            Name = "Summer Vacation 2026", 
            CreatedAt = new DateTime(2026, 5, 10),
            Duration = TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(34)
        });
        
        Projects.Add(new VideoProject 
        { 
            Name = "Music Video", 
            CreatedAt = new DateTime(2026, 5, 9),
            Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(45)
        });
        
        Projects.Add(new VideoProject 
        { 
            Name = "Product Presentation", 
            CreatedAt = new DateTime(2026, 5, 8),
            Duration = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(20)
        });
 
        Projects.Add(new VideoProject 
        { 
            Name = "Holiday Video", 
            CreatedAt = new DateTime(2026, 5, 5),
            Duration = TimeSpan.FromSeconds(45)
        });
        
        Projects.Add(new VideoProject 
        { 
            Name = "Tech Review", 
            CreatedAt = new DateTime(2026, 5, 2),
            Duration = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(12)
        });
        
        Projects.Add(new VideoProject 
        { 
            Name = "Travel Vlog", 
            CreatedAt = new DateTime(2026, 4, 28),
            Duration = TimeSpan.FromMinutes(8) + TimeSpan.FromSeconds(30)
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
