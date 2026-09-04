namespace Orcking.Services;

public record ExamBlueprint(List<ExamQuestionBlueprint> Questions);

public record ExamQuestionBlueprint(int QuestionId, List<int> OptionIds);
