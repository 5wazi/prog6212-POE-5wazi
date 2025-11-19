using Microsoft.AspNetCore.Mvc;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;

        public HRController(AppDbContext context)
        {
            _context = context;
        }

        // HR Dashboard
        public IActionResult Dashboard()
        {
            var totalUsers = _context.Users.Count();
            var lecturers = _context.Users.Count(u => u.UserRole.RoleName == "Lecturer");
            var admins = _context.Users.Count(u => u.UserRole.RoleName != "Lecturer");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalLecturers = lecturers;
            ViewBag.TotalAdmins = admins;

            return View();
        }

        // Create User
        public IActionResult CreateUser()
        {
            ViewBag.RoleList = new SelectList(_context.UserRoles, "RoleID", "RoleName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = new SelectList(_context.UserRoles, "RoleID", "RoleName", user.RoleID);
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("ViewUsers");
        }

        // View All Users
        public IActionResult ViewUsers()
        {
            var users = _context.Users.Include(u => u.UserRole).ToList();
            return View(users);
        }

        // Edit User
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            ViewBag.RoleList = new SelectList(_context.UserRoles, "RoleID", "RoleName", user.RoleID);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = new SelectList(_context.UserRoles, "RoleID", "RoleName", user.RoleID);
                return View(user);
            }

            _context.Users.Update(user);
            _context.SaveChanges();
            return RedirectToAction("ViewUsers");
        }

        // Generate Reports (LINQ)
        public IActionResult Reports()
        {
            var reportData = _context.Claims
                .Include(c => c.User)
                .GroupBy(c => c.User.FullName)
                .Select(g => new LecturerReport
                {
                    Lecturer = g.Key,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.Total)
                }).ToList();

            return View(reportData);
        }

    }
}
