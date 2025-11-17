using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ContractMonthlyClaimSystem.Models
{
    public class UserRole
    {
        [Key]
        public int RoleID { get; set; }

        //public UserRole Role { get; set; }

        [Required]
        [StringLength(50)]
        public string? RoleName { get; set; } // e.g., Lecturer, ProgrammeCoordinator, AcademicManager

        // Navigation Property
        public ICollection<User>? Users { get; set; }
    }
}
