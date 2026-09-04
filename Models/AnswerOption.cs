using System.ComponentModel.DataAnnotations;

namespace Orcking.Models;

public class AnswerOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public Question? Question { get; set; }

    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
