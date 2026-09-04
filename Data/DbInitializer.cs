using Microsoft.EntityFrameworkCore;
using Orcking.Models;
using Orcking.Services;

namespace Orcking.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var teacher = new ApplicationUser
        {
            Name = "Professora Mariana Rocha",
            Email = "professor@orcking.local",
            Role = UserRole.Teacher
        };

        var admin = new ApplicationUser
        {
            Name = "Administrador Orcking",
            Email = "admin@orcking.local",
            Role = UserRole.Admin
        };

        var classRoom = new ClassRoom
        {
            Name = "3A - Ensino Medio",
            Shift = "Manha"
        };

        var student = new ApplicationUser
        {
            Name = "Aluno Demo",
            Email = "aluno@orcking.local",
            RegistrationCode = "RA2026001",
            ClassRoom = classRoom,
            Role = UserRole.Student
        };

        var exam = new Exam
        {
            Title = "Prova Demo - Raciocinio Logico",
            Description = "Prova objetiva usada para validar o fluxo antifraude do MVP.",
            DurationMinutes = 45,
            ModelCount = 5,
            ApplicationDate = DateTime.Today,
            Status = ExamStatus.Published,
            Teacher = teacher,
            Questions =
            [
                BuildQuestion("Se todo A e B e algum B e C, qual afirmacao e necessariamente verdadeira?", "Logica", "Media", "Algum A pode ser C", ["Todo C e A", "Nenhum A e C", "Todo B e A"]),
                BuildQuestion("Qual numero completa a sequencia: 2, 4, 8, 16, ...?", "Sequencias", "Facil", "32", ["24", "30", "36"]),
                BuildQuestion("Uma turma tem 30 alunos e 40% sao mulheres. Quantas mulheres ha na turma?", "Porcentagem", "Facil", "12", ["10", "14", "18"]),
                BuildQuestion("Se uma prova tem peso 2 e nota 8, qual contribuicao ponderada?", "Media", "Facil", "16", ["8", "10", "12"]),
                BuildQuestion("Qual alternativa representa melhor uma medida antifraude baseada em auditoria?", "Seguranca", "Media", "Registrar eventos suspeitos com data e contexto", ["Bloquear todo teclado fisico", "Desligar o computador do aluno", "Permitir multiplas abas"]),
                BuildQuestion("Ao embaralhar alternativas, o que precisa ser preservado?", "Modelos", "Media", "O vinculo entre alternativa e gabarito", ["A letra original da alternativa", "A posicao visual antiga", "A ordem cadastrada pelo professor"])
            ]
        };

        db.Users.AddRange(teacher, admin, student);
        db.ClassRooms.Add(classRoom);
        db.Exams.Add(exam);
        await db.SaveChangesAsync();

        var generator = scope.ServiceProvider.GetRequiredService<ExamModelGenerator>();
        await generator.GenerateAsync(exam.Id, exam.ModelCount);
    }

    private static Question BuildQuestion(string statement, string topic, string difficulty, string correct, string[] incorrect)
    {
        var question = new Question
        {
            Statement = statement,
            Topic = topic,
            Difficulty = difficulty,
            Weight = 1
        };

        question.Options.Add(new AnswerOption { Text = correct, IsCorrect = true });
        question.Options.AddRange(incorrect.Select(text => new AnswerOption { Text = text }));
        return question;
    }
}
