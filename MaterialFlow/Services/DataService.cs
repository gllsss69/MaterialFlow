using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using MaterialFlow.Models;

namespace MaterialFlow.Services;

public class DataService
{
    public static DataService Instance { get; } = new DataService();

    private readonly string _dataFolderPath = Path.Combine(
        Path.GetDirectoryName(Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location)
        ?? AppDomain.CurrentDomain.BaseDirectory,
        "Data");
    
    // Шляхи до файлів згідно з ТЗ
    private readonly string _usersPath;
    private readonly string _projectsPath;
    private readonly string _platformsPath;
    private readonly string _presetsPath;
    private readonly string _jobsPath;
    private readonly string _outputFilesPath;

    public List<User> Users { get; private set; } = new();
    public List<VideoProject> Projects { get; private set; } = new();
    public List<Platform> Platforms { get; private set; } = new();
    public List<Preset> Presets { get; private set; } = new();
    public List<ConversionJob> Jobs { get; private set; } = new();
    public List<OutputFile> OutputFiles { get; private set; } = new();

    private DataService()
    {
        _usersPath = Path.Combine(_dataFolderPath, "users.json");
        _projectsPath = Path.Combine(_dataFolderPath, "projects.json");
        _platformsPath = Path.Combine(_dataFolderPath, "platforms.json");
        _presetsPath = Path.Combine(_dataFolderPath, "presets.json");
        _jobsPath = Path.Combine(_dataFolderPath, "jobs.json");
        _outputFilesPath = Path.Combine(_dataFolderPath, "outputfiles.json");

        EnsureDataFolderExists();
    }

    private void EnsureDataFolderExists()
    {
        if (!Directory.Exists(_dataFolderPath))
        {
            Directory.CreateDirectory(_dataFolderPath);
        }
    }

    /// <summary>
    /// Асинхронно завантажує та десеріалізує список об'єктів з JSON-файлу.
    /// </summary>
    /// <typeparam name="T">Тип елементів списку.</typeparam>
    /// <param name="filePath">Шлях до JSON-файлу.</param>
    /// <returns>Список об'єктів або пустий список у разі помилки.</returns>
    private async Task<List<T>> LoadListAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return new List<T>();

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    /// <summary>
    /// Асинхронно серіалізує та записує список об'єктів у JSON-файл.
    /// </summary>
    /// <typeparam name="T">Тип елементів списку.</typeparam>
    /// <param name="filePath">Шлях до файлу для збереження.</param>
    /// <param name="data">Список об'єктів.</param>
    private async Task SaveListAsync<T>(string filePath, List<T> data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving data to {filePath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Завантажує всі прикладні дані додатка при його запуску та ініціалізує стандартні платформи у разі потреби.
    /// </summary>
    public async Task LoadAllDataAsync()
    {
        Users = await LoadListAsync<User>(_usersPath);
        Projects = await LoadListAsync<VideoProject>(_projectsPath);
        Platforms = await LoadListAsync<Platform>(_platformsPath);
        Presets = await LoadListAsync<Preset>(_presetsPath);
        Jobs = await LoadListAsync<ConversionJob>(_jobsPath);
        OutputFiles = await LoadListAsync<OutputFile>(_outputFilesPath);

        if (Platforms.Count == 0)
        {
            InitializeDefaultPlatforms();
            await SavePlatformsAsync();
        }
        else
        {
            // Ensure "Other" platform exists for backward compatibility
            if (!Platforms.Any(p => p.Name.Equals("Other", StringComparison.OrdinalIgnoreCase)))
            {
                Platforms.Add(new Platform { Name = "Other", IconKind = "Web", DefaultResolution = "1920x1080" });
            }
            
            // Backfill icons
            foreach (var p in Platforms)
            {
                if (p.IconKind == "Web")
                {
                    if (p.Name == "YouTube") p.IconKind = "Youtube";
                    else if (p.Name == "TikTok") p.IconKind = "MusicNote";
                    else if (p.Name == "Instagram") p.IconKind = "Instagram";
                    else if (p.Name == "Facebook") p.IconKind = "Facebook";
                }
            }
            await SavePlatformsAsync();
        }

        // Створення всіх JSON-файлів, яких ще не існує, із порожнім масивом []
        await EnsureFileExistsAsync(_usersPath, Users);
        await EnsureFileExistsAsync(_projectsPath, Projects);
        await EnsureFileExistsAsync(_presetsPath, Presets);
        await EnsureFileExistsAsync(_jobsPath, Jobs);
        await EnsureFileExistsAsync(_outputFilesPath, OutputFiles);
    }

    /// <summary>
    /// Створює JSON-файл із поточними даними, якщо він ще не існує на диску.
    /// </summary>
    private async Task EnsureFileExistsAsync<T>(string filePath, List<T> data)
    {
        if (!File.Exists(filePath))
        {
            await SaveListAsync(filePath, data);
        }
    }

    /// <summary>
    /// Заповнює список платформ стандартними соціальними мережами при першому запуску програми.
    /// </summary>
    private void InitializeDefaultPlatforms()
    {
        Platforms.Add(new Platform { Name = "YouTube", IconKind = "Youtube", DefaultResolution = "1920x1080" });
        Platforms.Add(new Platform { Name = "TikTok", IconKind = "MusicNote", DefaultResolution = "720x1280" });
        Platforms.Add(new Platform { Name = "Instagram", IconKind = "Instagram", DefaultResolution = "1080x1080" });
        Platforms.Add(new Platform { Name = "Facebook", IconKind = "Facebook", DefaultResolution = "1080x1080" });
        Platforms.Add(new Platform { Name = "Other", IconKind = "Web", DefaultResolution = "1920x1080" });
    }

    /// <summary>
    /// Асинхронно зберігає список пресетів у presets.json.
    /// </summary>
    public async Task SavePresetsAsync() => await SaveListAsync(_presetsPath, Presets);

    /// <summary>
    /// Асинхронно зберігає список платформ у platforms.json.
    /// </summary>
    public async Task SavePlatformsAsync() => await SaveListAsync(_platformsPath, Platforms);

    /// <summary>
    /// Асинхронно зберігає чергу завдань у jobs.json.
    /// </summary>
    public async Task SaveJobsAsync() => await SaveListAsync(_jobsPath, Jobs);

    /// <summary>
    /// Асинхронно зберігає список вихідних файлів у outputfiles.json.
    /// </summary>
    public async Task SaveOutputFilesAsync() => await SaveListAsync(_outputFilesPath, OutputFiles);
}
