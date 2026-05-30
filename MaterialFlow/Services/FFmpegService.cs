using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using MaterialFlow.Models;

namespace MaterialFlow.Services;

/// <summary>
/// Сервіс для взаємодії з медіапроцесором FFmpeg для конвертування відео та генерації мініатюр.
/// </summary>
public class FFmpegService
{
    private readonly string _ffmpegPath;
    private readonly SemaphoreSlim _conversionQueue = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="FFmpegService"/> із зазначенням шляху до виконуваного файлу FFmpeg.
    /// </summary>
    /// <param name="ffmpegPath">Шлях до виконуваного файлу FFmpeg.</param>
    public FFmpegService(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }

    /// <summary>
    /// Перевіряє, чи доступний виконуваний файл FFmpeg у системі.
    /// </summary>
    /// <returns>Значення true, якщо FFmpeg доступний; інакше false.</returns>
    public bool IsFFmpegAvailable()
    {
        if (_ffmpegPath == "ffmpeg" || _ffmpegPath == "ffmpeg.exe")
            return true;
            
        return File.Exists(_ffmpegPath);
    }

    /// <summary>
    /// Асинхронно отримує версію FFmpeg для перевірки працездатності медіапроцесора.
    /// </summary>
    /// <returns>Рядок із версією FFmpeg або повідомленням про помилку.</returns>
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
    /// Асинхронно отримує загальну тривалість відеофайлу за допомогою запиту метаданих FFmpeg.
    /// </summary>
    /// <param name="filePath">Шлях до медіафайлу.</param>
    /// <returns>Об'єкт TimeSpan, що містить тривалість відеофайлу.</returns>
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
    /// Асинхронно конвертує відео відповідно до обраного пресету, повідомляє про прогрес та оновлює статус завдання.
    /// </summary>
    /// <param name="project">Модель відеопроєкту.</param>
    /// <param name="preset">Обраний пресет конфігурації рендерингу.</param>
    /// <param name="job">Об'єкт завдання конвертації для запису стану та прогресу.</param>
    /// <param name="progress">Об'єкт для повідомлення про зміну прогресу.</param>
    /// <param name="cancellationToken">Токен скасування асинхронної операції.</param>
    /// <returns>Значення true, якщо конвертація завершилась успішно; інакше false.</returns>
    public async Task<bool> ConvertVideoAsync(VideoProject project, Preset preset, ConversionJob job,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default, string? logFilePath = null)
    {
        await _conversionQueue.WaitAsync(cancellationToken);
        try
        {
            progress?.Report(0);

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
            // Визначаємо шлях до файлу водяного знаку
            var watermarkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Assets", "logo.png");
            bool applyWatermark = project.UseWatermark && File.Exists(watermarkPath);

            // Build arguments
            var args = "-y ";

            if (applyWatermark)
                args += $"-i \"{project.SourceFilePath}\" -i \"{watermarkPath}\" ";
            else
                args += $"-i \"{project.SourceFilePath}\" ";

            if (applyWatermark)
            {
                int watermarkWidth = 150;
                if (!string.IsNullOrWhiteSpace(preset.Resolution))
                {
                    var parts = preset.Resolution.Split('x', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2 && int.TryParse(parts[0], out int w))
                    {
                        watermarkWidth = Math.Max(50, w / 10);
                    }
                }
                string scaleWatermark = $"[1:v]scale={watermarkWidth}:-1[wm];";
                string overlayParams = "overlay=main_w-overlay_w-20:main_h-overlay_h-20[outv]";
                // filter_complex: масштабування + накладання водяного знаку в правому нижньому куті
                string filterComplex;
                if (!string.IsNullOrWhiteSpace(preset.Resolution))
                {
                    var dims = preset.Resolution.Replace("x", ":");
                    filterComplex = $"{scaleWatermark}[0:v]scale={dims}[scaled];[scaled][wm]{overlayParams}";
                }
                else
                {
                    filterComplex = $"{scaleWatermark}[0:v][wm]{overlayParams}";
                }
                args += $"-filter_complex \"{filterComplex}\" -map \"[outv]\" -map 0:a? ";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(preset.Resolution))
                    args += $"-s {preset.Resolution} ";
            }
            
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

            StreamWriter? logWriter = null;
            if (!string.IsNullOrEmpty(logFilePath))
            {
                try
                {
                    logWriter = new StreamWriter(logFilePath, append: true);
                    await logWriter.WriteLineAsync($"--- Starting Conversion: {DateTime.UtcNow} ---");
                    await logWriter.WriteLineAsync($"Command: {startInfo.FileName} {startInfo.Arguments}");
                }
                catch { }
            }

            // Register cancellation
            using var reg = cancellationToken.Register(() => {
                try {
                    if (!process.HasExited) process.Kill();
                } catch { }
            });

            try
            {
                // Read output asynchronously to report progress
                while (!process.StandardError.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    string? line = await process.StandardError.ReadLineAsync();
                    if (line != null)
                    {
                        if (logWriter != null)
                        {
                            try { await logWriter.WriteLineAsync(line); } catch { }
                        }

                        if (duration.TotalSeconds > 0)
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
                }

                await process.WaitForExitAsync(cancellationToken);
            }
            finally
            {
                if (logWriter != null)
                {
                    try
                    {
                        await logWriter.WriteLineAsync($"--- Finished Conversion: {DateTime.UtcNow} ---");
                        await logWriter.FlushAsync();
                        logWriter.Dispose();
                    }
                    catch { }
                }
            }

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
            job.ErrorMessage = "Conversion canceled.";
            return false;
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = $"Error during conversion: {ex.Message}";
            return false;
        }
        finally
        {
            _conversionQueue.Release();
        }
    }

    /// <summary>
    /// Асинхронно генерує зображення-обкладинку (мініатюру) для відео у вказаному часовому зміщенні.
    /// </summary>
    /// <param name="videoPath">Шлях до відеофайлу.</param>
    /// <param name="outputImagePath">Шлях для збереження створеного ескізу.</param>
    /// <param name="timeOffset">Часове зміщення кадру для мініатюри (наприклад, "00:00:01.000").</param>
    /// <returns>Шлях до згенерованого зображення, або пустий рядок у разі помилки.</returns>
    public async Task<string> GenerateThumbnailAsync(string videoPath, string outputImagePath, string timeOffset = "00:00:01.000")
    {
        if (!IsFFmpegAvailable() || !File.Exists(videoPath))
            return string.Empty;

        try
        {
            var args = $"-y -ss {timeOffset} -i \"{videoPath}\" -vframes 1 -q:v 2 \"{outputImagePath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return string.Empty;

            _ = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(outputImagePath))
            {
                return outputImagePath;
            }
        }
        catch
        {
            // Ignore errors, return empty string
        }

        return string.Empty;
    }
}
