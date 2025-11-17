using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }

        [Required, ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string? ModuleCode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }

        // HourlyRate is now read-only and automatically set from the User
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [StringLength(20)]
        public string? ClaimStatus { get; set; }

        public string? Notes { get; set; }

        // Navigation Properties
        public User? User { get; set; }

        public ICollection<Document>? Documents { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        // Method to update hourly rate and total from user
        public void SetHourlyRateFromUser()
        {
            if (User != null)
            {
                HourlyRate = User.HourlyRate;
                Total = HoursWorked * HourlyRate;
            }
        }
    }
}
