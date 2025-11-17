using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace ContractMonthlyClaimSystem.Models
{
    public class Document
    {
        [Key]
        public int DocumentID { get; set; }

        [Required]
        public int ClaimID { get; set; } // Foreign key

        [ForeignKey(nameof(ClaimID))]
        public Claim? Claim { get; set; } // Navigation property

        [Required]
        public string? FileName { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }
    }
}
