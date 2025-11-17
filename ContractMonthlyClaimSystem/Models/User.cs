using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, ForeignKey("UserRole")]
        public int RoleID { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string? UserEmail { get; set; }

        [Required]
        public string? Password { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        // HourlyRate for lecturers
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; } 

        // Navigation Properties
        public virtual UserRole? UserRole { get; set; }
        public ICollection<Claim>? Claims { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
