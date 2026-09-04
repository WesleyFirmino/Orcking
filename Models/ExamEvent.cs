using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class ExamEvent
{
    public int Id { get; set; }

    public int ExamAttemptId { get; set; }

    public ExamAttempt? ExamAttempt { get; set; }

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(80)]
    public string EventType { get; set; } = string.Empty;

    public ViolationSeverity Severity { get; set; } = ViolationSeverity.Info;

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(80)]
    public string? IpAddress { get; set; }

    [MaxLength(300)]
    public string? UserAgent { get; set; }
}
