using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;
using Orcking.ViewModels;

namespace Orcking.Controllers;

public class AccountController(AppDbContext db) : Controller
{
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> StudentLogin(LoginViewModel model)
    {
        var normalizedName = model.StudentFullName.Trim().ToUpperInvariant();
        var user = await db.Users
            .Include(item => item.ClassRoom)
            .FirstOrDefaultAsync(item => item.Role == UserRole.Student && item.Name.ToUpper() == normalizedName);

        if (user is null)
        {
            ModelState.AddModelError(nameof(model.StudentFullName), "Aluno nao encontrado. Confira o nome completo informado.");
            return View("Login", model);
        }

        SignIn(user);
        return RedirectToAction("Index", "Student");
    }

    [HttpPost]
    public async Task<IActionResult> StaffLogin(LoginViewModel model)
    {
        var normalizedEmail = model.StaffEmail.Trim().ToUpperInvariant();
        var user = await db.Users.FirstOrDefaultAsync(item =>
            (item.Role == UserRole.Teacher || item.Role == UserRole.Admin) && item.Email.ToUpper() == normalizedEmail);

        if (user is null)
        {
            ModelState.AddModelError(nameof(model.StaffEmail), "Login administrativo nao encontrado.");
            return View("Login", model);
        }

        SignIn(user);
        return RedirectToAction("Index", "Professor");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private void SignIn(ApplicationUser user)
    {
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());
        if (!string.IsNullOrWhiteSpace(user.RegistrationCode))
        {
            HttpContext.Session.SetString("RegistrationCode", user.RegistrationCode);
        }
    }
}
