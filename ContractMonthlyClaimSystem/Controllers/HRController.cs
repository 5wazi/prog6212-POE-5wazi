using Microsoft.AspNetCore.Mvc;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

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

            var recentUsers = _context.Users
                .Include(u => u.UserRole)
                .OrderByDescending(u => u.UserID)
                .Take(5)
                .ToList();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalLecturers = lecturers;
            ViewBag.TotalAdmins = admins;
            ViewBag.RecentUsers = recentUsers;

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
        public IActionResult Reports(int? month, int? year)
        {
            // Base query including user
            var claimsQuery = _context.Claims
                .Include(c => c.User)
                .AsQueryable();

            // Filter by month/year if provided
            if (month.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Month == month.Value);

            if (year.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Year == year.Value);

            // Group by lecturer
            var reportData = claimsQuery
                .GroupBy(c => c.User.FullName)
                .Select(g => new LecturerReport
                {
                    Lecturer = g.Key,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.Total),
                    Claims = g.ToList() // Keep all individual claims
                })
                .ToList();

            // Prepare month/year dropdowns with selected value
            ViewBag.Months = new SelectList(
                Enumerable.Range(1, 12)
                          .Select(m => new { Value = m, Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) }),
                "Value", "Text", month); // <-- selected month

            ViewBag.Years = new SelectList(
                _context.Claims
                    .Select(c => c.SubmissionDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .Select(y => new { Value = y, Text = y }),
                "Value", "Text", year); // <-- selected year

            return View(reportData);
        }


        public IActionResult DownloadReport(int? month, int? year)
        {
            // Get the same report data
            var claimsQuery = _context.Claims.Include(c => c.User).AsQueryable();

            if (month.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Month == month.Value);
            if (year.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Year == year.Value);

            var reportData = claimsQuery
                .GroupBy(c => c.User.FullName)
                .Select(g => new LecturerReport
                {
                    Lecturer = g.Key,
                    Claims = g.ToList(),
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalAmount = g.Sum(c => c.Total)
                }).ToList();

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new iText.Layout.Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("HR Lecturer Report")
                .SetFontSize(18)
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.CENTER));

            foreach (var report in reportData)
            {
                document.Add(new Paragraph($"Lecturer: {report.Lecturer}")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetMarginTop(15));

                var table = new Table(6, true);
                table.AddHeaderCell("Claim ID");
                table.AddHeaderCell("Module");
                table.AddHeaderCell("Hours Worked");
                table.AddHeaderCell("Amount");
                table.AddHeaderCell("Status");
                table.AddHeaderCell("Submitted On");

                foreach (var claim in report.Claims)
                {
                    table.AddCell(claim.ClaimID.ToString());
                    table.AddCell(claim.ModuleCode ?? "");
                    table.AddCell(claim.HoursWorked.ToString());
                    table.AddCell(claim.Total.ToString("C"));
                    table.AddCell(claim.ClaimStatus ?? "");
                    table.AddCell(claim.SubmissionDate.ToString("yyyy-MM-dd"));
                }

                document.Add(table);

                document.Add(new Paragraph($"Total Hours: {report.TotalHours}   Total Amount: {report.TotalAmount:C}")
                    .SetFont(boldFont)
                    .SetMarginBottom(10));
            }

            document.Close();

            var pdfBytes = ms.ToArray();
            return File(pdfBytes, "application/pdf", $"HR_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}
