using Microsoft.AspNetCore.Mvc;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ProgramCoordinatorController : Controller
    {
        private readonly AppDbContext _context;

        public ProgramCoordinatorController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Coordinator Dashboard
        public IActionResult Dashboard()
        {
            // Total claims
            var totalClaims = _context.Claims.Count();

            // Claims by status
            var pendingClaims = _context.Claims.Count(c => c.ClaimStatus == "Pending" || c.ClaimStatus == "Submitted");
            var approvedClaims = _context.Claims.Count(c => c.ClaimStatus == "Approved");
            var rejectedClaims = _context.Claims.Count(c => c.ClaimStatus == "Rejected");

            // Recent claim activity
            var recentClaims = _context.Claims
                                       .Include(c => c.User)
                                       .OrderByDescending(c => c.SubmissionDate)
                                       .Take(5)
                                       .ToList();

            // Lecturer overview
            var totalLecturers = _context.Users
                                 .Include(u => u.UserRole)
                                 .Count(u => u.UserRole.RoleName == "Lecturer");
            var activeLecturers = _context.Users
                                  .Include(u => u.UserRole)
                                  .Where(u => u.UserRole.RoleName == "Lecturer"
                                              && u.Claims.Any(c => c.SubmissionDate.Month == DateTime.Now.Month))
                                  .Count();
            var inactiveLecturers = totalLecturers - activeLecturers;

            // Pass data to the view
            ViewData["TotalClaims"] = totalClaims;
            ViewData["PendingClaims"] = pendingClaims;
            ViewData["ApprovedClaims"] = approvedClaims;
            ViewData["RejectedClaims"] = rejectedClaims;

            ViewData["RecentClaims"] = recentClaims;

            ViewData["TotalLecturers"] = totalLecturers;
            ViewData["ActiveLecturers"] = activeLecturers;
            ViewData["InactiveLecturers"] = inactiveLecturers;

            return View();
        }


        // GET: Claims awaiting verification
        public IActionResult Claims()
        {
            // Example: load claims that are submitted and need verification
            var claims = _context.Claims
                                 .Include(c => c.User)
                                 .Include(c => c.Documents)
                                 .Where(c => c.ClaimStatus == "Submitted" || c.ClaimStatus == "Pending")
                                 .OrderByDescending(c => c.SubmissionDate)
                                 .ToList();

            return View(claims);
        }

        // GET: Review a specific claim
        public IActionResult ReviewClaim(int id)
        {
            var claim = _context.Claims
                                .Include(c => c.Documents)
                                .Include(c => c.User)
                                .FirstOrDefault(c => c.ClaimID == id);

            if (claim == null)
                return NotFound();

            return View(claim);
        }

        // POST: Handle verification or rejection
        [HttpPost]
        public IActionResult ReviewClaim(int id, string action, string reviewerComment)
        {
            var claim = _context.Claims.Include(c => c.User).FirstOrDefault(c => c.ClaimID == id);
            if (claim == null)
                return NotFound();

            if (action == "Verify")
                claim.ClaimStatus = "Verified";
            else if (action == "Reject")
                claim.ClaimStatus = "Rejected";

            // Record the review (optional)
            var review = new Review
            {
                ClaimID = claim.ClaimID,
                UserID = 1, // Replace with current Program Coordinator's ID
                Comment = reviewerComment,
                ReviewType = "Verification",
                ReviewStatus = claim.ClaimStatus,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.Claims.Update(claim);
            _context.SaveChanges();

            return RedirectToAction("Claims");
        }
    }
}
