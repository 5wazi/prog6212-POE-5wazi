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
            var userId = 1; // replace with the actual authenticated user ID
            var user = _context.Users.Find(userId);

            if (user == null)
                return BadRequest("User not found.");

            var claim = new Claim
            {
                UserID = user.UserID,
                User = user,
                HourlyRate = user.HourlyRate, // pull from DB
                SubmissionDate = DateTime.Now
            };

            return View(claim);
        }


        // POST: AddClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClaim(Claim claim, List<IFormFile>? files)
        {
            //Console.WriteLine("Form posted!");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Invalid model");
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine(string.Join(", ", errors));

                return View(claim);
            }

            // Get the authenticated lecturer
            var userId = 1; // replace with actual authenticated user ID
            var user = _context.Users.Find(userId);
            if (user == null)
                return BadRequest("User not found.");

            // Assign claim properties
            claim.UserID = user.UserID;
            claim.FullName = user.FullName;
            claim.ClaimStatus = "Submitted";
            claim.SubmissionDate = DateTime.Now;

            // Calculate Total using lecturer's hourly rate
            claim.Total = claim.HoursWorked * user.HourlyRate;

            // Add claim to DB
            _context.Claims.Add(claim);
            _context.SaveChanges();

            // Handle file uploads
            if (files != null && files.Count > 0)
            {
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                List<string> failedFiles = new List<string>();
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                const long maxFileSize = 20 * 1024 * 1024; // 20 MB

                foreach (var file in files)
                {
                    try
                    {
                        if (file.Length > 0)
                        {
                            var ext = Path.GetExtension(file.FileName)?.ToLower();

                            // Validate file type
                            if (!allowedExtensions.Contains(ext))
                            {
                                failedFiles.Add($"{file.FileName} (Invalid type)");
                                continue;
                            }

                            // Validate file size
                            if (file.Length > maxFileSize)
                            {
                                failedFiles.Add($"{file.FileName} (Exceeds 20MB)");
                                continue;
                            }

                            // Save file
                            string filePath = Path.Combine(uploadPath, file.FileName);
                            using var stream = new FileStream(filePath, FileMode.Create);
                            file.CopyTo(stream);

                            // Add to DB
                            _context.Documents.Add(new Document
                            {
                                ClaimID = claim.ClaimID,
                                FileName = file.FileName,
                                UploadDate = DateTime.Now
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to upload {file.FileName}: {ex.Message}");
                        failedFiles.Add($"{file.FileName} (Error during upload)");
                    }
                }

                _context.SaveChanges();

                if (failedFiles.Any())
                {
                    ViewBag.UploadError = "The following files could not be uploaded: " + string.Join(", ", failedFiles);
                    return View(claim);
                }
            }
            else
            {
                ViewBag.UploadError = "No files were uploaded.";
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
