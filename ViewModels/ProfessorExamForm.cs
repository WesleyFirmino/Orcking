using System.ComponentModel.DataAnnotations;

namespace Orcking.ViewModels;

public class ProfessorExamForm
{
    public int? Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(5, 300)]
    public int DurationMinutes { get; set; } = 60;

    [Range(1, 10)]
    public int ModelCount { get; set; } = 5;
}
