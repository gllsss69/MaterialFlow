using System;

namespace MaterialFlow.Models;

public class ConversionJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid PresetId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending;
    public string OutputPath { get; set; } = string.Empty;
    public double Progress { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
