using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orcking.Data;
using Orcking.Models;
using Orcking.ViewModels;

namespace Orcking.Controllers;

public class AccountController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Login()
    {
        ViewBag.Users = await db.Users.OrderBy(user => user.Role).ThenBy(user => user.Name).ToListAsync();
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await db.Users.FindAsync(model.UserId);
        if (user is null)
        {
            ModelState.AddModelError("", "Selecione um usuario valido.");
            ViewBag.Users = await db.Users.OrderBy(item => item.Role).ThenBy(item => item.Name).ToListAsync();
            return View(model);
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserRole", user.Role.ToString());

        return user.Role == UserRole.Teacher
            ? RedirectToAction("Index", "Professor")
            : RedirectToAction("Index", "Student");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
