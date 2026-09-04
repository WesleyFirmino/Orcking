namespace Orcking.Models;

public class StudentAnswer
{
    public int Id { get; set; }

    public int ExamAttemptId { get; set; }

    public ExamAttempt? ExamAttempt { get; set; }

    public int QuestionId { get; set; }

    public Question? Question { get; set; }

    public int? AnswerOptionId { get; set; }

    public AnswerOption? AnswerOption { get; set; }
}
