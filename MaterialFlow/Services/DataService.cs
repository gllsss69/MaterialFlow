using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MaterialFlow.Models;

namespace MaterialFlow.Services;

public class DataService
{
    private readonly string _dataFolderPath = "Data";
    
    // Шляхи до файлів згідно з ТЗ
    private readonly string _usersPath;
    private readonly string _projectsPath;
    private readonly string _platformsPath;
    private readonly string _presetsPath;
    private readonly string _jobsPath;

    public List<User> Users { get; private set; } = new();
    public List<VideoProject> Projects { get; private set; } = new();
    public List<Platform> Platforms { get; private set; } = new();
    public List<Preset> Presets { get; private set; } = new();
    public List<ConversionJob> Jobs { get; private set; } = new();

    public DataService()
    {
        _usersPath = Path.Combine(_dataFolderPath, "users.json");
        _projectsPath = Path.Combine(_dataFolderPath, "projects.json");
        _platformsPath = Path.Combine(_dataFolderPath, "platforms.json");
        _presetsPath = Path.Combine(_dataFolderPath, "presets.json");
        _jobsPath = Path.Combine(_dataFolderPath, "jobs.json");

        EnsureDataFolderExists();
    }

    private void EnsureDataFolderExists()
    {
        if (!Directory.Exists(_dataFolderPath))
        {
            Directory.CreateDirectory(_dataFolderPath);
        }
    }

    // Універсальний метод для завантаження даних
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

    // Універсальний метод для збереження даних
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

    // Метод для завантаження ВСІХ даних при старті програми
    public async Task LoadAllDataAsync()
    {
        Users = await LoadListAsync<User>(_usersPath);
        Projects = await LoadListAsync<VideoProject>(_projectsPath);
        Platforms = await LoadListAsync<Platform>(_platformsPath);
        Presets = await LoadListAsync<Preset>(_presetsPath);
        Jobs = await LoadListAsync<ConversionJob>(_jobsPath);
    }
}
