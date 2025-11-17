using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }

        [Required, ForeignKey("Claim")]
        public int ClaimID { get; set; }

        [Required, ForeignKey("User")]
        public int UserID { get; set; } // Reviewer (Coordinator or Manager)

        public string? Comment { get; set; }

        [Required]
        public string? ReviewType { get; set; } // Verification or Approval

        public string? ReviewStatus { get; set; } // Pending, Approved, Rejected

        [Required]
        public DateTime ReviewDate { get; set; }

        // Navigation Properties
        public Claim? Claim { get; set; }
        public User? User { get; set; }
    }
}
