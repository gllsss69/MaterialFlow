using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Layout;
using MaterialFlow.Models;
using MaterialFlow.Services;

namespace MaterialFlow.ViewModels;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set 
            { 
                _currentUser = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(IsLoggedOut));
                OnPropertyChanged(nameof(UserFullName));
                OnPropertyChanged(nameof(UserInitials));
            }
        }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsLoggedOut => CurrentUser == null;
        public string UserFullName => CurrentUser?.FullName ?? "Guest";
        public string UserInitials => string.IsNullOrWhiteSpace(CurrentUser?.FullName) ? "?" : 
            new string(CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s[0]).ToArray()).ToUpper();

        private bool _isSidebarCollapsed;
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set
            {
                _isSidebarCollapsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SidebarWidth));
                OnPropertyChanged(nameof(DividerWidth));
                OnPropertyChanged(nameof(ItemWidth));
                OnPropertyChanged(nameof(SidebarContentAlignment));
            }
        }

        public double SidebarWidth => IsSidebarCollapsed ? 104 : 280;
        public double DividerWidth => IsSidebarCollapsed ? 56 : 232;
        public double ItemWidth => IsSidebarCollapsed ? 80 : 232;
        public double ItemHeight => 56;
        public HorizontalAlignment SidebarContentAlignment => IsSidebarCollapsed ? HorizontalAlignment.Center : HorizontalAlignment.Left;

        private int _currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => _currentPageIndex;
            set { _currentPageIndex = value; OnPropertyChanged(); }
        }

        // Settings
        private string _selectedLanguage = "English";
        private string _selectedTheme = "System Default";
        private string _defaultSavePath = "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(); }
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set { _selectedTheme = value; OnPropertyChanged(); }
        }

        public string DefaultSavePath
        {
            get => _defaultSavePath;
            set { _defaultSavePath = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Languages { get; } = new() { "English", "Ukrainian", "German", "French" };
        public ObservableCollection<string> Themes { get; } = new() { "System Default", "Light", "Dark" };

        public void Logout()
        {
            AuthService.Instance.Logout();
            CurrentUser = null;
        }

        public void SetPageIndex(string index)
        {
            if (int.TryParse(index, out int i))
            {
                CurrentPageIndex = i;
            }
        }

        public void ToggleSidebar() => IsSidebarCollapsed = !IsSidebarCollapsed;

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
