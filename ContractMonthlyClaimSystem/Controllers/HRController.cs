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
        public IActionResult Reports(int? month, string? lecturer, string? status)
        {
            var claimsQuery = _context.Claims
                .Include(c => c.User)
                .AsQueryable();

            // Filter by month if provided
            if (month.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Month == month.Value);

            // Filter by lecturer if provided
            if (!string.IsNullOrEmpty(lecturer) && lecturer != "All")
                claimsQuery = claimsQuery.Where(c => c.User.FullName == lecturer);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
                claimsQuery = claimsQuery.Where(c => c.ClaimStatus == status);

            // Group by lecturer
            var reportData = claimsQuery
                .GroupBy(c => c.User.FullName)
                .Select(g => new LecturerReport
                {
                    Lecturer = g.Key,
                    
                    // Sum only hours of filtered claims
                    TotalHours = g.Sum(c => c.HoursWorked),
                    // Sum only total of filtered claims
                    TotalAmount = g.Sum(c => c.Total),
                    Claims = g.ToList()
                })
                .ToList();

            // Month dropdown
            ViewBag.Months = new SelectList(
                Enumerable.Range(1, 12)
                          .Select(m => new { Value = m, Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) }),
                "Value", "Text", month);

            // Lecturer dropdown
            var lecturers = _context.Users
                .Where(u => u.UserRole.RoleName == "Lecturer")
                .Select(u => u.FullName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            lecturers.Insert(0, "All"); // Add "All" option
            ViewBag.Lecturers = new SelectList(lecturers, lecturer);

            // Status dropdown
            var statuses = new List<string> { "All", "Pending", "Approved", "Rejected", "Verified" };
            ViewBag.Statuses = new SelectList(statuses, status);

            // Save selected values
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedLecturer = lecturer;
            ViewBag.SelectedStatus = status;

            return View(reportData);
        }

        //Download PDF
        public IActionResult DownloadReport(int? month, string? lecturer, string? status)
        {
            var claimsQuery = _context.Claims.Include(c => c.User).AsQueryable();

            if (month.HasValue)
                claimsQuery = claimsQuery.Where(c => c.SubmissionDate.Month == month.Value);

            if (!string.IsNullOrEmpty(lecturer) && lecturer != "All")
                claimsQuery = claimsQuery.Where(c => c.User.FullName == lecturer);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                var statusNormalized = status.Trim().ToLower();
                claimsQuery = claimsQuery.Where(c => !string.IsNullOrEmpty(c.ClaimStatus)
                                                    && c.ClaimStatus.Trim().ToLower() == statusNormalized);
            }

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
                .SetFontSize(18).SetFont(boldFont).SetTextAlignment(TextAlignment.CENTER));

            foreach (var report in reportData)
            {
                document.Add(new Paragraph($"Lecturer: {report.Lecturer}")
                    .SetFont(boldFont).SetFontSize(14).SetMarginTop(15));

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
                    .SetFont(boldFont).SetMarginBottom(10));
            }

            document.Close();

            // Build dynamic filename
            string lecturerPart = !string.IsNullOrEmpty(lecturer) && lecturer != "All" ? lecturer : "All Lecturers";
            string monthPart = month.HasValue ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Value) : "All Months";
            string statusPart = !string.IsNullOrEmpty(status) && status != "All" ? status : "All Statuses";

            string fileName = $"{lecturerPart} - {monthPart} - {statusPart} Claims Report.pdf";

            return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}
