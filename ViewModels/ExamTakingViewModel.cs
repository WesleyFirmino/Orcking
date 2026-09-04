using Orcking.Models;
using Orcking.Services;

namespace Orcking.ViewModels;

public class ExamTakingViewModel
{
    public ExamAttempt Attempt { get; set; } = new();
    public ExamBlueprint Blueprint { get; set; } = new([]);
    public Dictionary<int, Question> Questions { get; set; } = [];
    public Dictionary<int, AnswerOption> Options { get; set; } = [];
    public Dictionary<int, int?> SelectedAnswers { get; set; } = [];
    public string Watermark { get; set; } = string.Empty;
    public DateTime EndsAtUtc { get; set; }
}
