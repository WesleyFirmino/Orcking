using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;
using Orcking.Services;
using Orcking.ViewModels;

namespace Orcking.Controllers;

public class ExamsController(AppDbContext db, ExamScoringService scoring) : Controller
{
    public async Task<IActionResult> Prepare(int id)
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var exam = await db.Exams
            .Include(item => item.Questions)
            .FirstOrDefaultAsync(item => item.Id == id && item.Status == ExamStatus.Published);

        if (exam is null) return NotFound();
        if (exam.ApplicationDate.Date != DateTime.Today)
        {
            return RedirectToAction("Index", "Student");
        }

        return View(exam);
    }

    public async Task<IActionResult> Start(int id)
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var studentId = CurrentUserId();
        var existing = await db.ExamAttempts
            .Where(item => item.ExamId == id && item.StudentId == studentId && (item.Status == AttemptStatus.InProgress || item.Status == AttemptStatus.BlockedByViolation))
            .OrderByDescending(item => item.StartedAtUtc)
            .FirstOrDefaultAsync();

        if (existing is not null)
        {
            if (existing.Status == AttemptStatus.BlockedByViolation)
            {
                return RedirectToAction(nameof(Resume), new { examId = id });
            }

            return RedirectToAction(nameof(Take), new { id = existing.Id });
        }

        var closed = await db.ExamAttempts
            .AnyAsync(item => item.ExamId == id && item.StudentId == studentId && item.Status == AttemptStatus.ZeroedByViolation);

        if (closed)
        {
            return RedirectToAction("Index", "Student");
        }

        var exam = await db.Exams.Include(item => item.Models).FirstOrDefaultAsync(item => item.Id == id);
        if (exam is null || exam.Status != ExamStatus.Published || exam.Models.Count == 0) return NotFound();
        if (exam.ApplicationDate.Date != DateTime.Today)
        {
            return RedirectToAction("Index", "Student");
        }

        var model = exam.Models.OrderBy(item => Guid.NewGuid()).First();
        var attempt = new ExamAttempt
        {
            ExamId = exam.Id,
            StudentId = studentId,
            ExamModelId = model.Id,
            Status = AttemptStatus.InProgress
        };

        db.ExamAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Take), new { id = attempt.Id });
    }

    public async Task<IActionResult> Resume(int examId)
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var attempt = await db.ExamAttempts
            .Where(item => item.ExamId == examId && item.StudentId == CurrentUserId())
            .OrderByDescending(item => item.StartedAtUtc)
            .FirstOrDefaultAsync();

        if (attempt is null) return RedirectToAction(nameof(Start), new { id = examId });

        if (attempt.Status == AttemptStatus.BlockedByViolation)
        {
            attempt.Status = AttemptStatus.InProgress;
            db.ExamEvents.Add(BuildEvent(attempt.Id, "resume_after_violation", ViolationSeverity.Warning, "Aluno retomou a prova apos a primeira violacao."));
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Take), new { id = attempt.Id });
        }

        return RedirectToAction(nameof(Take), new { id = attempt.Id });
    }

    public async Task<IActionResult> Take(int id)
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var attempt = await LoadAttempt(id);
        if (attempt is null || attempt.StudentId != CurrentUserId()) return NotFound();

        if (attempt.Status != AttemptStatus.InProgress)
        {
            return View("Result", attempt);
        }

        var blueprint = JsonSerializer.Deserialize<ExamBlueprint>(attempt.ExamModel!.BlueprintJson) ?? new ExamBlueprint([]);
        var questions = attempt.Exam!.Questions.ToDictionary(item => item.Id);
        var options = attempt.Exam.Questions.SelectMany(item => item.Options).ToDictionary(item => item.Id);

        return View(new ExamTakingViewModel
        {
            Attempt = attempt,
            Blueprint = blueprint,
            Questions = questions,
            Options = options,
            SelectedAnswers = attempt.Answers.ToDictionary(item => item.QuestionId, item => item.AnswerOptionId),
            EndsAtUtc = attempt.StartedAtUtc.AddMinutes(attempt.Exam.DurationMinutes),
            Watermark = $"{attempt.Student!.Name} | {attempt.Student.RegistrationCode} | Tentativa {attempt.Id} | {DateTime.Now:dd/MM/yyyy HH:mm}"
        });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerRequest request)
    {
        if (!RequireStudent()) return Unauthorized();

        var attempt = await db.ExamAttempts
            .Include(item => item.Answers)
            .FirstOrDefaultAsync(item => item.Id == request.AttemptId && item.StudentId == CurrentUserId());

        if (attempt is null || attempt.Status != AttemptStatus.InProgress) return BadRequest();

        var answer = attempt.Answers.FirstOrDefault(item => item.QuestionId == request.QuestionId);
        if (answer is null)
        {
            attempt.Answers.Add(new StudentAnswer { QuestionId = request.QuestionId, AnswerOptionId = request.OptionId });
        }
        else
        {
            answer.AnswerOptionId = request.OptionId;
        }

        await db.SaveChangesAsync();
        return Ok(new { saved = true });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReportEvent([FromBody] ExamEventRequest request)
    {
        if (!RequireStudent()) return Unauthorized();

        var attempt = await db.ExamAttempts.FirstOrDefaultAsync(item => item.Id == request.AttemptId && item.StudentId == CurrentUserId());
        if (attempt is null || attempt.Status != AttemptStatus.InProgress) return BadRequest();

        var severity = request.Severity?.Equals("critical", StringComparison.OrdinalIgnoreCase) == true
            ? ViolationSeverity.Critical
            : ViolationSeverity.Warning;

        attempt.Events.Add(BuildEvent(attempt.Id, request.EventType, severity, request.Details));

        if (severity == ViolationSeverity.Critical)
        {
            attempt.ViolationCount += 1;
            if (attempt.ViolationCount >= 2)
            {
                attempt.Status = AttemptStatus.ZeroedByViolation;
                attempt.Score = 0;
                attempt.SubmittedAtUtc = DateTime.UtcNow;
            }
            else
            {
                attempt.Status = AttemptStatus.BlockedByViolation;
            }
        }

        await db.SaveChangesAsync();
        return Ok(new { status = attempt.Status.ToString(), violations = attempt.ViolationCount });
    }

    [HttpPost]
    public async Task<IActionResult> Submit(int id)
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var attempt = await LoadAttempt(id);
        if (attempt is null || attempt.StudentId != CurrentUserId()) return NotFound();
        if (attempt.Status != AttemptStatus.InProgress) return View("Result", attempt);

        attempt.Score = await scoring.ScoreAsync(attempt.Id);
        attempt.Status = AttemptStatus.Submitted;
        attempt.SubmittedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return View("Result", attempt);
    }

    private async Task<ExamAttempt?> LoadAttempt(int id)
    {
        return await db.ExamAttempts
            .Include(item => item.Student)
            .Include(item => item.ExamModel)
            .Include(item => item.Answers)
            .ThenInclude(item => item.Question)
            .Include(item => item.Exam)
            .ThenInclude(item => item!.Questions)
            .ThenInclude(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    private ExamEvent BuildEvent(int attemptId, string eventType, ViolationSeverity severity, string? details)
    {
        return new ExamEvent
        {
            ExamAttemptId = attemptId,
            EventType = eventType,
            Severity = severity,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };
    }

    private bool RequireStudent() => HttpContext.Session.GetString("UserRole") == UserRole.Student.ToString();
    private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

    public record SaveAnswerRequest(int AttemptId, int QuestionId, int OptionId);
    public record ExamEventRequest(int AttemptId, string EventType, string? Severity, string? Details);
}
