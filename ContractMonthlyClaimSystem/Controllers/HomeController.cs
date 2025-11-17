using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ContractMonthlyClaimSystem.Models;
using Azure.Identity;
using System.ComponentModel.DataAnnotations;
using ContractMonthlyClaimSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
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

    public IActionResult FeatureComingSoon(string featureName, string userRole)
    {
        ViewData["FeatureName"] = featureName;
        ViewData["UserRole"] = userRole; // Pass the current user role
        return View();
    }


    // GET: Home/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View(); 
    }

    // POST: Home/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(Login model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Find user and include role
        var user = _context.Users
            .Include(u => u.UserRole)
            .FirstOrDefault(u => u.UserEmail == model.UserEmail && u.Password == model.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // Redirect based on user’s role
        switch (user.UserRole.RoleName)
        {
            case "Lecturer":
                return RedirectToAction("Dashboard", "Lecturer");

            case "Coordinator":
                return RedirectToAction("Dashboard", "ProgramCoordinator");

            case "Manager":
                return RedirectToAction("Dashboard", "AcademicManager");

            default:
                ModelState.AddModelError("", "Invalid role assigned to this account.");
                return View(model);
        }
    }


    // Optional: Logout action
    public IActionResult Logout()
    {
        return RedirectToAction("Login", "Home");
    }
}
