using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class Exam
{
    public int Id { get; set; }

    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int DurationMinutes { get; set; } = 60;

    public int ModelCount { get; set; } = 5;

    public ExamStatus Status { get; set; } = ExamStatus.Draft;

    public int TeacherId { get; set; }

    public ApplicationUser? Teacher { get; set; }

    public List<Question> Questions { get; set; } = [];

    public List<ExamModel> Models { get; set; } = [];
}
