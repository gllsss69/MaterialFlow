using System;

namespace MaterialFlow.Models;

public class Platform
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string IconKind { get; set; } = "Web";
    
    // Default settings for the platform
    public string DefaultResolution { get; set; } = "1920x1080";
    public int DefaultBitrate { get; set; } = 5000;
    public string DefaultFormat { get; set; } = "mp4";
    public string DefaultAspectRatio { get; set; } = "16:9";
    public int DefaultFPS { get; set; } = 30;
    public string DefaultCodec { get; set; } = "libx264";
}
