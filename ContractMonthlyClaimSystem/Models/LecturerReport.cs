namespace ContractMonthlyClaimSystem.Models
{
    public class LecturerReport
    {
        public string Lecturer { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
