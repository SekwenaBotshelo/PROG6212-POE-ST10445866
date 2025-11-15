using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        // Foreign key to User (instead of storing lecturer name/rate directly)
        public int LecturerId { get; set; }
        public virtual User Lecturer { get; set; } = null!;

        [Required(ErrorMessage = "Please enter total hours worked.")]
        [Range(1, 180, ErrorMessage = "Hours must be between 1 and 180.")]
        public double TotalHours { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending Verification";

        // Auto-calculated property using lecturer's hourly rate
        public double TotalAmount => TotalHours * (double)Lecturer.HourlyRate;

        // Approval tracking
        public int? VerifiedByCoordinatorId { get; set; }
        public virtual User? VerifiedByCoordinator { get; set; }

        public int? ApprovedByManagerId { get; set; }
        public virtual User? ApprovedByManager { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? VerifiedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }
}