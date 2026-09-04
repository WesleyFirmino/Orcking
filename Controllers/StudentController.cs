using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;
using Orcking.ViewModels;

namespace Orcking.Controllers;

public class StudentController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!RequireStudent()) return RedirectToAction("Login", "Account");

        var student = await db.Users
            .Include(item => item.ClassRoom)
            .FirstAsync(item => item.Id == CurrentUserId());

        var exams = await db.Exams
            .Where(item => item.Status == ExamStatus.Published)
            .OrderBy(item => item.ApplicationDate)
            .ThenBy(item => item.Title)
            .ToListAsync();

        var attempts = await db.ExamAttempts
            .Where(item => item.StudentId == CurrentUserId())
            .ToListAsync();

        return View(new StudentDashboardViewModel
        {
            Student = student,
            Exams = exams,
            Attempts = attempts,
            Today = DateTime.Today
        });
    }

    private bool RequireStudent() => HttpContext.Session.GetString("UserRole") == UserRole.Student.ToString();
    private int CurrentUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;
}
