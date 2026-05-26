using System;
using System.IO;
using System.Text.Json;

namespace MaterialFlow.Services;

/// <summary>
/// Сервіс для збереження та завантаження налаштувань користувача з файлу settings.json.
/// </summary>
public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly string _settingsPath;

    private SettingsService()
    {
        _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
    }

    public string SelectedLanguage { get; set; } = "English";
    public string SelectedTheme { get; set; } = "System Default";
    public string DefaultSavePath { get; set; } = "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";
    public string LastLoginUser { get; set; } = string.Empty;
    public bool EnableProjectLogging { get; set; }

    /// <summary>
    /// Зберігає поточні налаштування у файл settings.json.
    /// </summary>
    public void Save()
    {
        try
        {
            var settings = new
            {
                SelectedLanguage,
                SelectedTheme,
                DefaultSavePath,
                LastLoginUser,
                EnableProjectLogging
            };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Завантажує налаштування з файлу settings.json.
    /// </summary>
    public void Load()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return;

            var json = File.ReadAllText(_settingsPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("SelectedLanguage", out var langProp))
                SelectedLanguage = langProp.GetString() ?? "English";

            if (root.TryGetProperty("SelectedTheme", out var themeProp))
                SelectedTheme = themeProp.GetString() ?? "System Default";

            if (root.TryGetProperty("DefaultSavePath", out var pathProp))
                DefaultSavePath = pathProp.GetString() ?? "C:\\Users\\maksim\\Documents\\MaterialFlow\\Projects";

            if (root.TryGetProperty("LastLoginUser", out var userProp))
                LastLoginUser = userProp.GetString() ?? string.Empty;

            if (root.TryGetProperty("EnableProjectLogging", out var logProp))
                EnableProjectLogging = logProp.ValueKind == JsonValueKind.True;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
    }
}
