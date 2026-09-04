using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Orcking.Models;

namespace Orcking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("UserRole") is "Teacher" or "Admin")
        {
            return RedirectToAction("Index", "Professor");
        }

        if (HttpContext.Session.GetString("UserRole") == "Student")
        {
            return RedirectToAction("Index", "Student");
        }

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
