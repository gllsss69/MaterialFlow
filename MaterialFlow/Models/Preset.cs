using System;

namespace MaterialFlow.Models;

public class Preset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid PlatformId { get; set; }
    
    public string Resolution { get; set; } = "1920x1080";
    public int Bitrate { get; set; } = 5000;
    public string Codec { get; set; } = "libx264";
    public int FrameRate { get; set; } = 30;
    public int AudioBitrate { get; set; } = 128;
}
