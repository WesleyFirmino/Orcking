using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;

namespace Orcking.Controllers;

public class StudentController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var exams = await db.Exams
            .Include(item => item.Questions)
            .Where(item => item.Status == ExamStatus.Published)
            .OrderBy(item => item.Title)
            .ToListAsync();

        var attempts = await db.ExamAttempts
            .Where(item => item.StudentId == CurrentUserId())
            .ToListAsync();

        ViewBag.Attempts = attempts;
        return View(exams);
    }

    private bool RequireStudent() => HttpContext.Session.GetString("UserRole") == UserRole.Student.ToString();
    private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
}
