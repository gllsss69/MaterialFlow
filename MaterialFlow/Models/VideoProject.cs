using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MaterialFlow.Models;

/// <summary>
/// Представляє модель проєкту відеообробки з усіма його характеристиками та станом обробки.
/// </summary>
public class VideoProject : INotifyPropertyChanged
{
    private double _progress;
    private string _statusText = string.Empty;

    /// <summary>
    /// Унікальний ідентифікатор проєкту.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Назва відеопроєкту.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Файловий шлях до вихідного (джерельного) відео.
    /// </summary>
    public string SourceFilePath { get; set; } = string.Empty;

    private string _exportFilePath = string.Empty;

    /// <summary>
    /// Файловий шлях для експорту обробленого відео.
    /// </summary>
    public string ExportFilePath
    {
        get => _exportFilePath;
        set { _exportFilePath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Роздільна здатність відео (наприклад, "1920x1080").
    /// </summary>
    public string Resolution { get; set; } = "1920x1080";

    /// <summary>
    /// Бітрейт вихідного відео (наприклад, "5000 kbps").
    /// </summary>
    public string Bitrate { get; set; } = "5000 kbps";

    /// <summary>
    /// Формат/контейнер файлу (наприклад, "mp4").
    /// </summary>
    public string Format { get; set; } = "mp4";

    /// <summary>
    /// Частота кадрів відео (наприклад, "60 fps").
    /// </summary>
    public string FPS { get; set; } = "60 fps";

    /// <summary>
    /// Платформа для публікації (наприклад, "YouTube", "TikTok").
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// Кодек відео (наприклад, "libx264").
    /// </summary>
    public string Codec { get; set; } = "libx264";



    /// <summary>
    /// Вказує, чи потрібно накладати водяний знак під час рендерингу.
    /// </summary>
    public bool UseWatermark { get; set; } = true;

    private string _thumbnailPath = string.Empty;

    /// <summary>
    /// Файловий шлях до обкладинки/ескізу відео.
    /// </summary>
    public string ThumbnailPath
    {
        get => _thumbnailPath;
        set { _thumbnailPath = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Тривалість відео.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Дата та час створення проєкту.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Ідентифікатор користувача, якому належить проєкт.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Поточний прогрес обробки проєкту (від 0.0 до 100.0).
    /// </summary>
    [JsonIgnore]
    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Текстовий опис поточного стану обробки (наприклад, "Очікування...", "Рендеринг...").
    /// </summary>
    [JsonIgnore]
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private bool _isProcessing;

    /// <summary>
    /// Визначає, чи виконується в даний момент обробка (конвертація) цього проєкту.
    /// </summary>
    [JsonIgnore]
    public bool IsProcessing
    {
        get => _isProcessing;
        set { _isProcessing = value; OnPropertyChanged(); }
    }

    private bool _isFavorite;

    /// <summary>
    /// Визначає, чи є проєкт обраним/улюбленим.
    /// </summary>
    public bool IsFavorite
    {
        get => _isFavorite;
        set { _isFavorite = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Допоміжний метод для викликання події зміни властивості.
    /// </summary>
    /// <param name="propertyName">Назва властивості, що змінилась.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
