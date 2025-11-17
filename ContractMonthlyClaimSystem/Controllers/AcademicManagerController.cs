using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class AcademicManagerController : Controller
    {
        private readonly AppDbContext _context;

        public AcademicManagerController(AppDbContext context)
        {
            _context = context;
        }

        // Manager Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalClaims = await _context.Claims.CountAsync();
            ViewBag.PendingFinalApproval = await _context.Claims.CountAsync(c => c.ClaimStatus == "Verified");
            ViewBag.CompletedClaims = await _context.Claims.CountAsync(c => c.ClaimStatus == "Approved" || c.ClaimStatus == "Rejected");

            var recentDecisions = await _context.Claims
                .Include(c => c.User)
                .Where(c => c.ClaimStatus == "Approved" || c.ClaimStatus == "Rejected")
                .OrderByDescending(c => c.SubmissionDate)
                .Take(5)
                .ToListAsync();

            ViewData["RecentDecisions"] = recentDecisions;
            return View();
        }

        // Pending claims for final approval
        public async Task<IActionResult> Claims()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .Where(c => c.ClaimStatus == "Verified")
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(pendingClaims);
        }

        // GET: Review a specific claim
        public async Task<IActionResult> ReviewClaim(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimID == id);

            if (claim == null)
                return NotFound();

            return View(claim);
        }

        // POST: Handle approval/rejection
        [HttpPost]
        public async Task<IActionResult> ReviewClaim(int id, string action, string comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
                return NotFound();

            if (action == "Approve")
                claim.ClaimStatus = "Approved";
            else if (action == "Reject")
                claim.ClaimStatus = "Rejected";

            // Record manager review
            var review = new Review
            {
                ClaimID = claim.ClaimID,
                UserID = 2, // Replace with current Academic Manager's ID
                Comment = comment,
                ReviewType = "FinalApproval",
                ReviewStatus = claim.ClaimStatus,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction("Claims");
        }
    }
}
