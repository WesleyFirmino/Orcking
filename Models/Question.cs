using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class Question
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    public Exam? Exam { get; set; }

    [MaxLength(2000)]
    public string Statement { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Topic { get; set; }

    [MaxLength(40)]
    public string? Difficulty { get; set; }

    public decimal Weight { get; set; } = 1;

    public List<AnswerOption> Options { get; set; } = [];
}
