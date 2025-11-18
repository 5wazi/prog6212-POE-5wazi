using Microsoft.AspNetCore.Mvc;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class LecturerController : Controller
    {
        private readonly AppDbContext _context;

        public LecturerController(AppDbContext context)
        {
            _context = context;
        }

        //Part 3.2
        private int GetLoggedInUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                // Not logged in → redirect to login page
                RedirectToAction("Login", "Home");
                return 0; // failsafe
            }

            return userId.Value;
        }


        // GET: Lecturer Dashboard
        public IActionResult Dashboard()
        {
            var userId = 1; // user ID from authentication
            var claims = _context.Claims
                                 .Where(c => c.UserID == userId)
                                 .Include(c => c.Documents)
                                 .OrderByDescending(c => c.SubmissionDate)
                                 .ToList();

            // Dashboard stats
            var totalClaims = claims.Count;
            var pending = claims.Count(c => c.ClaimStatus == "Pending");
            var approved = claims.Count(c => c.ClaimStatus == "Approved");
            var rejected = claims.Count(c => c.ClaimStatus == "Rejected");

            ViewBag.TotalClaims = totalClaims;
            ViewBag.PendingClaims = pending;
            ViewBag.ApprovedClaims = approved;
            ViewBag.RejectedClaims = rejected;

            return View(claims); // pass claims to the view
        }


        // GET: My Claims
        public IActionResult Claims()
        {
            var userId = 1; //user id
            var claims = _context.Claims
                                 .Where(c => c.UserID == userId)
                                 .Include(c => c.Documents)
                                 .OrderByDescending(c => c.SubmissionDate)
                                 .ToList();

            return View(claims);
        }

        // GET: AddClaim
        public IActionResult AddClaim()
        {
            var userId = GetLoggedInUserId();
            var user = _context.Users.Find(userId);

            if (user == null)
                return BadRequest("User not found.");

            // DO NOT bind HourlyRate from form (prevents duplicate decimal errors)
            var claim = new Claim
            {
                UserID = user.UserID,
                FullName = user.FullName,
                HourlyRate = user.HourlyRate, 
                SubmissionDate = DateTime.Now
            };

            return View(claim);
        }


        // POST: AddClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClaim(Claim claim, List<IFormFile>? files)
        {
            var userId = GetLoggedInUserId();
            var user = _context.Users.Find(userId);

            if (user == null)
                return BadRequest("User not found.");

            // FORCE correct values (ignore anything from form for these fields)
            claim.UserID = user.UserID;
            claim.FullName = user.FullName;
            claim.HourlyRate = user.HourlyRate;
            claim.SubmissionDate = DateTime.Now;
            claim.ClaimStatus = "Submitted";

            // ---- VALIDATION: Max 180 hours ----
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "You cannot claim more than 180 hours in a month.");
            }

            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            // CALCULATE TOTAL
            claim.Total = claim.HoursWorked * claim.HourlyRate;

            // SAVE CLAIM
            _context.Claims.Add(claim);
            _context.SaveChanges();

            // ---------------- FILE UPLOADS ----------------

            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                List<string> failedFiles = new List<string>();
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                const long maxFileSize = 20 * 1024 * 1024;

                foreach (var file in files)
                {
                    try
                    {
                        var ext = Path.GetExtension(file.FileName).ToLower();

                        if (!allowedExtensions.Contains(ext))
                        {
                            failedFiles.Add($"{file.FileName} (Invalid type)");
                            continue;
                        }

                        if (file.Length > maxFileSize)
                        {
                            failedFiles.Add($"{file.FileName} (Exceeds 20MB)");
                            continue;
                        }

                        string filePath = Path.Combine(uploadPath, Guid.NewGuid() + ext);

                        using var stream = new FileStream(filePath, FileMode.Create);
                        file.CopyTo(stream);

                        _context.Documents.Add(new Document
                        {
                            ClaimID = claim.ClaimID,
                            FileName = Path.GetFileName(filePath),
                            UploadDate = DateTime.Now
                        });
                    }
                    catch
                    {
                        failedFiles.Add($"{file.FileName} (Failed to upload)");
                    }
                }

                _context.SaveChanges();

                if (failedFiles.Any())
                {
                    ViewBag.UploadError = "Some files failed: " + string.Join(", ", failedFiles);
                    return View(claim);
                }
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult TestRedirect()
        {
            return RedirectToAction("Dashboard");
        }

        // GET: View Claim Details
        public IActionResult ViewClaim(int id)
        {
            var claim = _context.Claims
                .Where(c => c.ClaimID == id)
                .Select(c => new Claim
                {
                    ClaimID = c.ClaimID,
                    SubmissionDate = c.SubmissionDate,
                    HoursWorked = c.HoursWorked,
                    //HourlyRate = c.HourlyRate,
                    Total = c.Total,
                    ClaimStatus = c.ClaimStatus,
                    Notes = c.Notes,
                    Documents = c.Documents
                })
                .FirstOrDefault();

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }
    }
}
