namespace Orcking.Models;

public class ExamAttempt
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    public Exam? Exam { get; set; }

    public int StudentId { get; set; }

    public ApplicationUser? Student { get; set; }

    public int ExamModelId { get; set; }

    public ExamModel? ExamModel { get; set; }

    public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAtUtc { get; set; }

    public int ViolationCount { get; set; }

    public decimal? Score { get; set; }

    public List<StudentAnswer> Answers { get; set; } = [];

    public List<ExamEvent> Events { get; set; } = [];
}
