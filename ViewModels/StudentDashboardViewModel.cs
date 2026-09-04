using Orcking.Models;

namespace Orcking.ViewModels;

public class StudentDashboardViewModel
{
    public ApplicationUser Student { get; set; } = new();
    public List<Exam> Exams { get; set; } = [];
    public List<ExamAttempt> Attempts { get; set; } = [];
    public DateTime Today { get; set; } = DateTime.Today;
}
