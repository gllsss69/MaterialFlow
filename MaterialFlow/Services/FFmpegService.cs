using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MaterialFlow.Services;

public class FFmpegService
{
    private readonly string _ffmpegPath;

    public FFmpegService(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    /// <summary>
    /// Перевіряє, чи доступний виконуваний файл FFmpeg
    /// </summary>
    public bool IsFFmpegAvailable()
    {
        return File.Exists(_ffmpegPath);
    }

    /// <summary>
    /// Отримує версію FFmpeg для перевірки працездатності
    /// </summary>
    public async Task<string> GetFFmpegVersionAsync()
    {
        if (!IsFFmpegAvailable())
            return "FFmpeg not found";

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return "Failed to start process";

            string output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Повертаємо перший рядок виводу, де зазвичай вказана версія
            return output.Split('\n')[0];
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
