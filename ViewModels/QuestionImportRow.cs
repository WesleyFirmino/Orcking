namespace Orcking.ViewModels;

public class QuestionImportRow
{
    public string Statement { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string OptionE { get; set; } = string.Empty;
    public string Correct { get; set; } = "A";
    public decimal Weight { get; set; } = 1;
    public string? Topic { get; set; }
    public string? Difficulty { get; set; }
}
