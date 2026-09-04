using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class ExamModel
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    public Exam? Exam { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public string BlueprintJson { get; set; } = string.Empty;

    public List<ExamAttempt> Attempts { get; set; } = [];
}
