using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;
using Orcking.Services;
using Orcking.ViewModels;

namespace Orcking.Controllers;

public class ProfessorController(AppDbContext db, ExamModelGenerator generator) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var exams = await db.Exams
            .Include(item => item.Questions)
            .Include(item => item.Models)
            .Include(item => item.Teacher)
            .OrderByDescending(item => item.Id)
            .ToListAsync();

        return View(exams);
    }

    public IActionResult Create()
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");
        return View(new ProfessorExamForm { ModelCount = 5, DurationMinutes = 60 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProfessorExamForm form)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(form);

        var exam = new Exam
        {
            Title = form.Title,
            Description = form.Description,
            DurationMinutes = form.DurationMinutes,
            ModelCount = form.ModelCount,
            ApplicationDate = form.ApplicationDate.Date,
            Status = ExamStatus.Draft,
            TeacherId = CurrentUserId()
        };

        db.Exams.Add(exam);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = exam.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var exam = await db.Exams
            .Include(item => item.Questions)
            .ThenInclude(item => item.Options)
            .Include(item => item.Models)
            .Include(item => item.Teacher)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (exam is null) return NotFound();
        return View(exam);
    }

    [HttpPost]
    public async Task<IActionResult> AddQuestion(int examId, QuestionImportRow row)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var exam = await db.Exams.Include(item => item.Questions).FirstOrDefaultAsync(item => item.Id == examId);
        if (exam is null) return NotFound();

        exam.Questions.Add(BuildQuestion(examId, row));
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = examId });
    }

    [HttpPost]
    public async Task<IActionResult> ImportCsv(int examId, IFormFile file)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");
        if (file.Length == 0) return RedirectToAction(nameof(Details), new { id = examId });

        using var reader = new StreamReader(file.OpenReadStream());
        var first = true;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (first && line.Contains("Enunciado", StringComparison.OrdinalIgnoreCase))
            {
                first = false;
                continue;
            }

            first = false;
            var columns = line.Split(';');
            if (columns.Length < 7) continue;

            db.Questions.Add(BuildQuestion(examId, new QuestionImportRow
            {
                Statement = columns[0],
                OptionA = columns[1],
                OptionB = columns[2],
                OptionC = columns[3],
                OptionD = columns[4],
                OptionE = columns.Length > 5 ? columns[5] : "",
                Correct = columns.Length > 6 ? columns[6] : "A",
                Weight = columns.Length > 7 && decimal.TryParse(columns[7], NumberStyles.Number, CultureInfo.InvariantCulture, out var weight) ? weight : 1,
                Topic = columns.Length > 8 ? columns[8] : null,
                Difficulty = columns.Length > 9 ? columns[9] : null
            }));
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = examId });
    }

    [HttpPost]
    public async Task<IActionResult> GenerateModels(int id)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var exam = await db.Exams.Include(item => item.Questions).ThenInclude(item => item.Options).FirstOrDefaultAsync(item => item.Id == id);
        if (exam is null) return NotFound();

        await generator.GenerateAsync(id, exam.ModelCount);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Publish(int id)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var exam = await db.Exams.FindAsync(id);
        if (exam is null) return NotFound();

        exam.Status = ExamStatus.Published;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Attempts(int id)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var attempts = await db.ExamAttempts
            .Include(item => item.Student)
            .Include(item => item.ExamModel)
            .Include(item => item.Events)
            .Where(item => item.ExamId == id)
            .OrderByDescending(item => item.StartedAtUtc)
            .ToListAsync();

        ViewBag.Exam = await db.Exams.FindAsync(id);
        return View(attempts);
    }

    public async Task<IActionResult> Students()
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var students = await db.Users
            .Include(user => user.ClassRoom)
            .Where(user => user.Role == UserRole.Student)
            .OrderBy(user => user.Name)
            .ToListAsync();

        ViewBag.Classes = await db.ClassRooms.OrderBy(item => item.Name).ToListAsync();
        return View(students);
    }

    [HttpPost]
    public async Task<IActionResult> AddStudent(string name, string email, string? registrationCode, int? classRoomId)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(Students));
        }

        var exists = await db.Users.AnyAsync(user => user.Email == email);
        if (!exists)
        {
            db.Users.Add(new ApplicationUser
            {
                Name = name.Trim(),
                Email = email.Trim(),
                RegistrationCode = registrationCode?.Trim(),
                ClassRoomId = classRoomId,
                Role = UserRole.Student
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Students));
    }

    [HttpPost]
    public async Task<IActionResult> ImportStudentsCsv(IFormFile file)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");
        if (file.Length == 0) return RedirectToAction(nameof(Students));

        using var reader = new StreamReader(file.OpenReadStream());
        var first = true;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (first && line.Contains("Nome", StringComparison.OrdinalIgnoreCase))
            {
                first = false;
                continue;
            }

            first = false;
            var columns = line.Split(';');
            if (columns.Length < 2) continue;

            var email = columns[1].Trim();
            if (await db.Users.AnyAsync(user => user.Email == email)) continue;

            db.Users.Add(new ApplicationUser
            {
                Name = columns[0].Trim(),
                Email = email,
                RegistrationCode = columns.Length > 2 ? columns[2].Trim() : null,
                ClassRoomId = columns.Length > 3 && int.TryParse(columns[3].Trim(), out var classRoomId) ? classRoomId : null,
                Role = UserRole.Student
            });
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Students));
    }

    public async Task<IActionResult> Classes()
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        var classes = await db.ClassRooms
            .Include(item => item.Students)
            .OrderBy(item => item.Name)
            .ToListAsync();

        return View(classes);
    }

    [HttpPost]
    public async Task<IActionResult> AddClass(string name, string? shift)
    {
        if (!RequireStaff()) return RedirectToAction("Login", "Account");

        if (!string.IsNullOrWhiteSpace(name))
        {
            db.ClassRooms.Add(new ClassRoom
            {
                Name = name.Trim(),
                Shift = shift?.Trim()
            });

            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Classes));
    }

    private static Question BuildQuestion(int examId, QuestionImportRow row)
    {
        var correct = row.Correct.Trim().ToUpperInvariant();
        var options = new[] { row.OptionA, row.OptionB, row.OptionC, row.OptionD, row.OptionE };

        return new Question
        {
            ExamId = examId,
            Statement = row.Statement,
            Topic = row.Topic,
            Difficulty = row.Difficulty,
            Weight = row.Weight <= 0 ? 1 : row.Weight,
            Options = options
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Select((text, index) => new AnswerOption { Text = text, IsCorrect = correct == ((char)('A' + index)).ToString() })
                .ToList()
        };
    }

    private bool RequireStaff() => HttpContext.Session.GetString("UserRole") is "Teacher" or "Admin";
    private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
}
