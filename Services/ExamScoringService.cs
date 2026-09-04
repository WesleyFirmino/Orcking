using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;

namespace Orcking.Services;

public class ExamScoringService(AppDbContext db)
{
    public async Task<decimal> ScoreAsync(int attemptId)
    {
        var attempt = await db.ExamAttempts
            .Include(item => item.Answers)
            .Include(item => item.Exam)
            .ThenInclude(item => item!.Questions)
            .ThenInclude(item => item.Options)
            .FirstAsync(item => item.Id == attemptId);

        var totalWeight = attempt.Exam!.Questions.Sum(item => item.Weight);
        if (totalWeight == 0)
        {
            return 0;
        }

        var questions = attempt.Exam.Questions.ToDictionary(question => question.Id);
        var options = attempt.Exam.Questions
            .SelectMany(question => question.Options)
            .ToDictionary(option => option.Id);

        var earned = attempt.Answers.Sum(answer =>
        {
            if (!answer.AnswerOptionId.HasValue || !questions.TryGetValue(answer.QuestionId, out var question))
            {
                return 0;
            }

            var option = options.GetValueOrDefault(answer.AnswerOptionId.Value);

            return option?.IsCorrect == true ? question.Weight : 0;
        });

        return Math.Round(earned / totalWeight * 10, 2);
    }
}
