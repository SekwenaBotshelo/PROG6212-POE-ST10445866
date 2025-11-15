using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PROG6212_POE.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        // Foreign key to User (Lecturer who submitted the claim)
        [Required]
        public int LecturerId { get; set; }

        [ForeignKey("LecturerId")]
        public virtual User Lecturer { get; set; } = null!;

        [Required(ErrorMessage = "Please enter total hours worked.")]
        [Range(1, 180, ErrorMessage = "Hours must be between 1 and 180.")]
        [Display(Name = "Hours Worked")]
        public double TotalHours { get; set; }

        // NOTES IS OPTIONAL - NO [Required] ATTRIBUTE
        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Claim Month")]
        public string Month { get; set; } = string.Empty;

        // Status workflow: Pending Verification → Verified → Approved → Paid
        public string Status { get; set; } = "Pending Verification";

        // Auto-calculated property with null checking
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public double TotalAmount => TotalHours * (double)(Lecturer?.HourlyRate ?? 0);

        // Approval tracking
        [Display(Name = "Verified By")]
        public int? VerifiedByCoordinatorId { get; set; }

        [ForeignKey("VerifiedByCoordinatorId")]
        public virtual User? VerifiedByCoordinator { get; set; }

        [Display(Name = "Approved By")]
        public int? ApprovedByManagerId { get; set; }

        [ForeignKey("ApprovedByManagerId")]
        public virtual User? ApprovedByManager { get; set; }

        // Timestamps
        [Display(Name = "Submitted Date")]
        [DataType(DataType.DateTime)]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Display(Name = "Verified Date")]
        [DataType(DataType.DateTime)]
        public DateTime? VerifiedDate { get; set; }

        [Display(Name = "Approved Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ApprovedDate { get; set; }

        // File upload for supporting documents
        [Display(Name = "Supporting Document")]
        public string? DocumentPath { get; set; }

        [Display(Name = "Document Original Name")]
        public string? DocumentOriginalName { get; set; }

        // COMPUTED PROPERTIES FOR BACKWARD COMPATIBILITY
        [NotMapped]
        public string LecturerName => Lecturer?.FullName ?? "Unknown Lecturer";

        [NotMapped]
        public decimal HourlyRate => Lecturer?.HourlyRate ?? 0;
    }
}