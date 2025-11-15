using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Please enter your name.")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter total hours worked.")]
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200.")]
        public double TotalHours { get; set; }

        [Required(ErrorMessage = "Please enter your hourly rate.")]
        [Range(100, 1000, ErrorMessage = "Hourly rate must be between R100 and R1000.")]
        public double HourlyRate { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string Month { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending Verification";

        public double TotalAmount => TotalHours * HourlyRate;
    }
}