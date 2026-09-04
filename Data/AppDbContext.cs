using Microsoft.EntityFrameworkCore;
using Orcking.Models;

namespace Orcking.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<ExamModel> ExamModels => Set<ExamModel>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
    public DbSet<ExamEvent> ExamEvents => Set<ExamEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<Question>()
            .Property(question => question.Weight)
            .HasPrecision(8, 2);

        modelBuilder.Entity<ExamAttempt>()
            .Property(attempt => attempt.Score)
            .HasPrecision(8, 2);

        modelBuilder.Entity<ExamAttempt>()
            .HasIndex(attempt => new { attempt.ExamId, attempt.StudentId });

        modelBuilder.Entity<StudentAnswer>()
            .HasIndex(answer => new { answer.ExamAttemptId, answer.QuestionId })
            .IsUnique();
    }
}
