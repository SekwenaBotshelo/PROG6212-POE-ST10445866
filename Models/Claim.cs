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
        public virtual User? Lecturer { get; set; } // Make nullable

        [Required(ErrorMessage = "Please enter total hours worked.")]
        [Range(1, 180, ErrorMessage = "Hours must be between 1 and 180.")]
        [Display(Name = "Hours Worked")]
        public double TotalHours { get; set; }

        // Store hourly rate at time of submission to calculate amount
        [Display(Name = "Hourly Rate")]
        public decimal StoredHourlyRate { get; set; }

        // Store calculated amount
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal StoredTotalAmount { get; set; }

        // NOTES IS OPTIONAL
        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Claim Month")]
        public string Month { get; set; } = string.Empty;

        // Status workflow: Pending Verification → Verified → Approved → Paid
        public string Status { get; set; } = "Pending Verification";

        // Computed property that uses stored amount or calculates if needed
        [NotMapped]
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public double TotalAmount
        {
            get
            {
                if (StoredTotalAmount > 0)
                    return (double)StoredTotalAmount;
                else
                    return TotalHours * (double)(Lecturer?.HourlyRate ?? StoredHourlyRate);
            }
        }

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

        // COMPUTED PROPERTIES WITH NULL CHECKING
        [NotMapped]
        public string LecturerName => Lecturer?.FullName ?? "Unknown Lecturer";

        [NotMapped]
        public decimal HourlyRate => StoredHourlyRate;

        // NEW: Safe property for Lecturer FullName
        [NotMapped]
        public string SafeLecturerName => Lecturer?.FullName ?? "Lecturer Not Found";

        // NEW: Safe property for Lecturer Email
        [NotMapped]
        public string SafeLecturerEmail => Lecturer?.Email ?? "Email Not Available";
    }
}