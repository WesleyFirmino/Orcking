using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;

namespace Orcking.Services;

public class ExamModelGenerator(AppDbContext db)
{
    public async Task GenerateAsync(int examId, int modelCount)
    {
        var exam = await db.Exams
            .Include(item => item.Questions)
            .ThenInclude(item => item.Options)
            .Include(item => item.Models)
            .FirstAsync(item => item.Id == examId);

        db.ExamModels.RemoveRange(exam.Models);

        var questions = exam.Questions.OrderBy(item => item.Id).ToList();
        for (var index = 1; index <= modelCount; index++)
        {
            var random = new Random(HashCode.Combine(examId, index, questions.Count));
            var blueprint = new ExamBlueprint(
                questions
                    .OrderBy(_ => random.Next())
                    .Select(question => new ExamQuestionBlueprint(
                        question.Id,
                        question.Options.OrderBy(_ => random.Next()).Select(option => option.Id).ToList()))
                    .ToList());

            exam.Models.Add(new ExamModel
            {
                Code = ((char)('A' + index - 1)).ToString(),
                BlueprintJson = JsonSerializer.Serialize(blueprint)
            });
        }

        exam.ModelCount = modelCount;
        await db.SaveChangesAsync();
    }
}
