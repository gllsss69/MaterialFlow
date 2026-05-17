using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MaterialFlow.Models;

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
        if (_ffmpegPath == "ffmpeg" || _ffmpegPath == "ffmpeg.exe")
            return true;
            
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

    /// <summary>
    /// Отримує тривалість відеофайлу
    /// </summary>
    public async Task<TimeSpan> GetVideoDurationAsync(string filePath)
    {
        if (!IsFFmpegAvailable() || !File.Exists(filePath))
            return TimeSpan.Zero;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{filePath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return TimeSpan.Zero;

            string output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // FFmpeg outputs info to stderr. We look for "Duration: 00:00:00.00"
            var match = Regex.Match(output, @"Duration: (\d{2}):(\d{2}):(\d{2}\.\d{2})");
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                double seconds = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                return new TimeSpan(0, hours, minutes, 0, (int)(seconds * 1000));
            }

            return TimeSpan.Zero;
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Конвертує відео відповідно до пресету та оновлює статус завдання
    /// </summary>
    public async Task<bool> ConvertVideoAsync(VideoProject project, Preset preset, ConversionJob job, IProgress<double> progress = null, CancellationToken cancellationToken = default)
    {
        if (!IsFFmpegAvailable())
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "FFmpeg is not available.";
            return false;
        }

        if (!File.Exists(project.SourceFilePath))
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "Source file does not exist.";
            return false;
        }

        // Get duration if not already available
        var duration = project.Duration;
        if (duration.TotalSeconds == 0)
        {
            duration = await GetVideoDurationAsync(project.SourceFilePath);
        }

        job.Status = JobStatus.Processing;
        job.StartTime = DateTime.UtcNow;

        try
        {
            // Build arguments
            // Example: -i input.mp4 -s 1920x1080 -b:v 5000k -c:v libx264 -r 30 -b:a 128k output.mp4
            var args = $"-y -i \"{project.SourceFilePath}\" ";
            
            if (!string.IsNullOrWhiteSpace(preset.Resolution))
                args += $"-s {preset.Resolution} ";
            
            if (preset.Bitrate > 0)
                args += $"-b:v {preset.Bitrate}k ";
            
            if (!string.IsNullOrWhiteSpace(preset.Codec))
                args += $"-c:v {preset.Codec} ";
            
            if (preset.FrameRate > 0)
                args += $"-r {preset.FrameRate} ";
            
            if (preset.AudioBitrate > 0)
                args += $"-b:a {preset.AudioBitrate}k ";
                
            args += $"\"{job.OutputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = args,
                RedirectStandardError = true, // FFmpeg outputs progress to stderr
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Failed to start FFmpeg process.";
                return false;
            }

            // Register cancellation
            using var reg = cancellationToken.Register(() => {
                try {
                    if (!process.HasExited) process.Kill();
                } catch { }
            });

            // Read output asynchronously to report progress
            var buffer = new char[1024];
            while (!process.StandardError.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                string line = await process.StandardError.ReadLineAsync();
                if (line != null && duration.TotalSeconds > 0)
                {
                    // Look for time=00:00:00.00
                    var match = Regex.Match(line, @"time=(\d{2}):(\d{2}):(\d{2}\.\d{2})");
                    if (match.Success)
                    {
                        int hours = int.Parse(match.Groups[1].Value);
                        int minutes = int.Parse(match.Groups[2].Value);
                        double seconds = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                        var currentTime = new TimeSpan(0, hours, minutes, 0, (int)(seconds * 1000));
                        
                        double currentProgress = (currentTime.TotalSeconds / duration.TotalSeconds) * 100.0;
                        if (currentProgress > 100) currentProgress = 100;
                        if (currentProgress < 0) currentProgress = 0;
                        
                        job.Progress = Math.Round(currentProgress, 2);
                        progress?.Report(job.Progress);
                    }
                }
            }

            await process.WaitForExitAsync(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Conversion was cancelled.";
                return false;
            }

            if (process.ExitCode == 0)
            {
                job.Status = JobStatus.Completed;
                job.EndTime = DateTime.UtcNow;
                job.Progress = 100;
                progress?.Report(100);
                return true;
            }
            else
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = $"FFmpeg exited with code {process.ExitCode}";
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = "Conversion was cancelled.";
            return false;
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = $"Error during conversion: {ex.Message}";
            return false;
        }
    }
}
